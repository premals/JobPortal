using JobSeekerService.Application.EventHandler;
using JobSeekerService.Application.Interfaces;
using JobSeekerService.Application.UseCases;
using JobSeekerService.Infrastructure.AI;
using JobSeekerService.Infrastructure.Resume;
using JobSeekerService.Infrastructure.Messaging;
using JobSeekerService.Infrastructure.Messaging.AzureServiceBus;
using JobSeekerService.Infrastructure.Messaging.RabbitMq;
using JobSeekerService.Infrastructure.Mongo;
using JobSeekerService.Infrastructure.Mongo.Indexes;
using JobSeekerService.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using NLog;
using NLog.Config;
using NLog.Web;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;

LogManager.Setup().SetupExtensions(ext =>
    ext.RegisterTarget<AzureLogAnalyticsTarget>("AzureLogAnalytics"));

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseNLog();

Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "logs"));

// ================================
// MongoDB Configuration
// ================================
builder.Services.AddOptions();

builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("Mongo"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

// =======================
// MongoDbContext
// =======================
builder.Services.AddScoped<MongoDbContext>();

// ================================
// Dependency Injection – Repositories
// ================================
builder.Services.AddScoped<IJobReadRepository, JobReadRepository>();
builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
builder.Services.AddScoped<IJobSeekerRepository, JobSeekerRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IResumeDraftRepository, ResumeDraftRepository>();
//builder.Services.AddSingleton<JobReadIndexSeeder>();
//builder.Services.AddSingleton<JobSeekerIndexSeeder>();
//builder.Services.AddSingleton<JobApplicationIndexSeeder>();

// ================================
// Dependency Injection – Use Cases
// ================================
builder.Services.AddScoped<BrowseJobsUseCase>();
builder.Services.AddScoped<GetJobDetailsUseCase>();
builder.Services.AddScoped<ApplyJobUseCase>();
builder.Services.AddScoped<GetMyApplicationsUseCase>();
builder.Services.AddScoped<WithdrawJobApplicationUseCase>();
builder.Services.AddScoped<GenerateResumeAiUseCase>();
builder.Services.AddScoped<ParseResumeUseCase>();
builder.Services.AddScoped<ParseResumeFileUseCase>();
// ================================
// AI Services
// ================================
builder.Services.AddScoped<IResumeAiService, ResumeAiService>();
builder.Services.AddScoped<IResumePdfGenerator, ResumePdfGenerator>();
builder.Services.Configure<OpenAiOptions>(
    builder.Configuration.GetSection("OpenAI"));
builder.Services.AddHttpClient<IResumeParserService, ResumeParserService>();

// ================================
// Authentication (JWT)
// ================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),

            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };
    });

// ================================
// Authorization
// ================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireJobSeeker",
        policy => policy.RequireRole("JobSeeker"));
});


var messagingProvider = builder.Configuration["Messaging:Provider"];
var bootstrapLogger = LogManager.GetCurrentClassLogger();

if (messagingProvider == "RabbitMQ")
{
    builder.Services.AddHostedService<JobEventsRabbitConsumer>();
    builder.Services.AddSingleton<IEventBus, RabbitMqEventBus>();
}
else if (messagingProvider == "AzureServiceBus")
{
    builder.Services.AddHostedService<JobEventsServiceBusConsumer>();
    builder.Services.AddSingleton<IEventBus, AzureServiceBusEventBus>();
}
else
{
    bootstrapLogger.Error("Unsupported Messaging Provider: {Provider}. Messaging is disabled.", messagingProvider);
    builder.Services.AddSingleton<IEventBus, NoOpEventBus>();
}

builder.Services.AddScoped<JobSeekerRegisteredEventHandler>();
builder.Services.AddScoped<JobApplicationStatusUpdatedEventHandler>();
builder.Services.AddScoped<InterviewInviteCreatedEventHandler>();

if (messagingProvider == "RabbitMQ")
{
    builder.Services.AddHostedService<JobSeekerRegisteredConsumer>();
}
else if (messagingProvider == "AzureServiceBus")
{
    builder.Services.AddHostedService<JobSeekerRegisteredServiceBusConsumer>();
}

// ================================
// Controllers & Swagger
// ================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = feature?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        if (exception != null)
        {
            logger.LogError(exception, "Unhandled exception for {Path}", feature?.Path);
        }

        var statusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            ApplicationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        if (exception is InvalidOperationException invalidOp &&
            invalidOp.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            statusCode = StatusCodes.Status404NotFound;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var title = statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status404NotFound => "Not Found",
            _ => "Internal Server Error"
        };

        var detail = statusCode == StatusCodes.Status500InternalServerError
            ? "An unexpected error occurred."
            : exception?.Message ?? title;

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        });
    });
});

app.Use(async (context, next) =>
{
    var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    using (MappedDiagnosticsLogicalContext.SetScoped("traceId", traceId))
    using (MappedDiagnosticsLogicalContext.SetScoped("requestId", context.TraceIdentifier))
    {
        await next();
    }
});

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

    await new JobReadIndexSeeder(database).CreateIndexesAsync();
    await new JobSeekerIndexSeeder(database).CreateIndexesAsync();
    await new JobApplicationIndexSeeder(database).CreateIndexesAsync();
    await new NotificationIndexSeeder(database).CreateIndexesAsync();
    await new ResumeDraftIndexSeeder(database).CreateIndexesAsync();
}

// ================================
// Middleware Pipeline (ORDER MATTERS)
// ================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();   // MUST be before UseAuthorization
app.UseAuthorization();

app.MapControllers();
app.Run();







