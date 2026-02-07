using AdminService.Infrastructure.Mongo;
using AdminService.Infrastructure.Mongo.Indexes;
using AdminService.Infrastructure.Repositories;
using AdminService.Infrastructure.Messaging;
using AdminService.Infrastructure.Messaging.RabbitMq;
using AdminService.Infrastructure.Messaging.AzureServiceBus;
using AdminService.Logging;
using AdminService.Infrastructure.Sync;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using NLog;
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

builder.Services.AddScoped<MongoDbContext>();
builder.Services.AddScoped<AdminReadRepository>();
builder.Services.AddScoped<AdminEventHandler>();

// ================================
// Admin Profile Sync
// ================================
builder.Services.Configure<AdminSyncOptions>(
    builder.Configuration.GetSection("Sync"));
builder.Services.AddHostedService<AdminProfileSyncHostedService>();

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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin",
        policy => policy.RequireRole("Admin"));
});

// ================================
// Messaging
// ================================
var messagingProvider = builder.Configuration["Messaging:Provider"];
var bootstrapLogger = LogManager.GetCurrentClassLogger();

if (messagingProvider == "RabbitMQ")
{
    builder.Services.AddHostedService<JobEventsConsumer>();
    builder.Services.AddHostedService<JobApplicationsConsumer>();
    builder.Services.AddHostedService<IdentityEventsConsumer>();
}
else if (messagingProvider == "AzureServiceBus")
{
    builder.Services.AddSingleton<ServiceBusClient>(sp =>
        new ServiceBusClient(
            builder.Configuration["Messaging:AzureServiceBus:ConnectionString"]
        ));

    builder.Services.AddHostedService<JobEventsServiceBusConsumer>();
    builder.Services.AddHostedService<JobApplicationsServiceBusConsumer>();
    builder.Services.AddHostedService<IdentityEventsServiceBusConsumer>();
}
else
{
    bootstrapLogger.Error("Unsupported Messaging Provider: {Provider}. Messaging is disabled.", messagingProvider);
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
    await new AdminIndexSeeder(database).CreateIndexesAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
