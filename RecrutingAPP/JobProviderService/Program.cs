using Azure.Messaging.ServiceBus;
using JobProviderService.Application;
using JobProviderService.Application.EventHandlers;
using JobProviderService.Application.Interfaces;
using JobProviderService.Application.UseCases;
using JobProviderService.Domain;
using JobProviderService.Infrastructure;
using JobProviderService.Infrastructure.AI;
using JobProviderService.Infrastructure.Indexes;
using JobProviderService.Infrastructure.Messaging;
using JobProviderService.Infrastructure.Messaging.Azure;
using JobProviderService.Infrastructure.Messaging.RabbitMQ;
using JobProviderService.Infrastructure.Options;
using JobProviderService.Infrastructure.Repository;
using JobProviderService.Infrastructure.Services;
using JobProviderService.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using NLog;
using NLog.Config;
using NLog.Web;
using System.Text;
using System.Diagnostics;

LogManager.Setup().SetupExtensions(ext =>
    ext.RegisterTarget<AzureLogAnalyticsTarget>("AzureLogAnalytics"));

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseNLog();

Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "logs"));

// =======================
// MongoDB Configuration
// =======================
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
    return client.GetDatabase(settings.Database);
});

// =======================
// MongoDbContext
// =======================
builder.Services.AddScoped<MongoDbContext>();

// =======================
// Dependency Injection
// =======================
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
builder.Services.AddScoped<IInterviewRepository, InterviewRepository>();
builder.Services.AddScoped<IJobProviderSettingsRepository, JobProviderSettingsRepository>();
builder.Services.AddScoped<IJobProviderProfileRepository, JobProviderProfileRepository>();
builder.Services.AddScoped<CreateJobUseCase>();
builder.Services.AddScoped<UpdateJobUseCase>();
builder.Services.AddScoped<DeleteJobUseCase>();
builder.Services.AddScoped<UpdateJobPartialUseCase>();
builder.Services.AddScoped<CreateInterviewInviteUseCase>();
builder.Services.AddScoped<GetAiShortlistSuggestionUseCase>();
builder.Services.AddScoped<InterviewSessionUseCase>();
builder.Services.AddScoped<UpdateApplicationStatusUseCase>();
builder.Services.AddScoped<JobAppliedEventHandler>();
builder.Services.AddScoped<JobApplicationWithdrawnEventHandler>();
builder.Services.AddHttpClient<IEmailService, HttpEmailService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var baseUrl = config["EmailService:BaseUrl"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    }
});

builder.Services.Configure<OpenAiOptions>(
    builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<AzureOpenAiOptions>(
    builder.Configuration.GetSection("AzureOpenAI"));
builder.Services.Configure<AppUrlOptions>(
    builder.Configuration.GetSection("AppUrls"));
builder.Services.Configure<BlobStorageOptions>(
    builder.Configuration.GetSection("BlobStorage"));
builder.Services.AddHttpClient<IAiInterviewService, OpenAiInterviewService>();
builder.Services.AddSingleton<IInterviewRecordingStorage, AzureBlobInterviewRecordingStorage>();

// -----------------------------
// Messaging Configuration
// -----------------------------
var messagingProvider = builder.Configuration["Messaging:Provider"];
var bootstrapLogger = LogManager.GetCurrentClassLogger();

if (messagingProvider == "RabbitMQ")
{
    builder.Services.AddSingleton<IEventBus, RabbitMqEventBus>();
    builder.Services.AddHostedService<JobApplicationConsumer>();
}
else if (messagingProvider == "AzureServiceBus")
{
    builder.Services.AddSingleton<ServiceBusClient>(sp =>
        new ServiceBusClient(
            builder.Configuration["Messaging:AzureServiceBus:ConnectionString"]
        ));

    builder.Services.AddSingleton<IEventBus, AzureServiceBusEventBus>();
    builder.Services.AddHostedService<JobApplicationServiceBusConsumer>();
}
else
{
    bootstrapLogger.Error("Unsupported Messaging Provider: {Provider}. Messaging is disabled.", messagingProvider);
    builder.Services.AddSingleton<IEventBus, NoOpEventBus>();
}


// =======================
// Authentication (JWT)
// =======================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateLifetime = true
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(context.Exception, "JWT authentication failed.");
            return Task.CompletedTask;
        }
    };
});



// =======================
// Authorization
// =======================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireJobProvider",
        policy => policy.RequireRole("JobProvider"));
    options.AddPolicy("RequireJobSeeker",
        policy => policy.RequireRole("JobSeeker"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("SwaggerCorsPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5122/"   // API Gateway Swagger URL
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// =======================
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
    await new JobApplicationIndexSeeder(database).CreateIndexesAsync();
}

// =======================
// Middleware order (IMPORTANT)
// =======================
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
