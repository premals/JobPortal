using AspNetCore.Identity.MongoDbCore.Extensions;
using Azure.Messaging.ServiceBus;
using IdendityService.Helper;
using IdendityService.Infrastructure.Messaging;
using IdendityService.Infrastructure.Messaging.AzureServiceBus;
using IdendityService.Infrastructure.Messaging.RabbitMq;
using IdendityService.Logging;
using IdendityService.Interfaces;
using IdendityService.Interfaces.Auth;
using IdendityService.Models;
using IdendityService.Services;
using IdendityService.Services.UseCases;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using NLog;
using NLog.Config;
using NLog.Web;
using System.Text;
using System.Diagnostics;

LogManager.Setup().SetupExtensions(ext =>
    ext.RegisterTarget<AzureLogAnalyticsTarget>("AzureLogAnalytics"));

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.Logging.ClearProviders();
builder.Host.UseNLog();

Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "logs"));

// Identity (MongoDB)
services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
        configuration["MongoSettings:ConnectionString"],
        configuration["MongoSettings:DatabaseName"])
    .AddDefaultTokenProviders();

// Configure JWT auth
var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);
var client = new MongoClient(builder.Configuration["MongoSettings:ConnectionString"]);
var database = client.GetDatabase(builder.Configuration["MongoSettings:DatabaseName"]);
await SetupMongoIndexes.ConfigureMongoIndexes(database);

services.AddSingleton<IMongoClient>(client);
services.AddSingleton<IMongoDatabase>(database);
services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true
    };
});

// Authorization policies (example)
services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));

    options.AddPolicy("RequireJobSeeker",
        policy => policy.RequireRole("JobSeeker"));

    options.AddPolicy("RequireJobProvider",
        policy => policy.RequireRole("JobProvider"));
});

services.AddCors(options =>
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

// App services
services.AddScoped<IJwtService, JwtService>();
services.AddScoped<IRefreshTokenService, RefreshTokenService>();
services.AddScoped<IEmailService, EmailService>();
services.AddScoped<IForgotPasswordUseCase, ForgotPasswordUseCase>();
services.AddScoped<IResetPasswordUseCase, ResetPasswordUseCase>();
services.AddScoped<IConfirmEmailUseCase, ConfirmEmailUseCase>();
services.AddScoped<IAssignRoleUseCase, AssignRoleUseCase>();
services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
services.AddScoped<ILoginUseCase, LoginUseCase>();
services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
services.AddScoped<IConfirmEmailUseCase, ConfirmEmailUseCase>();
services.AddScoped<IAssignRoleUseCase, AssignRoleUseCase>();

var messagingProvider = builder.Configuration["Messaging:Provider"];
var bootstrapLogger = LogManager.GetCurrentClassLogger();

if (messagingProvider == "RabbitMQ")
{
    services.AddSingleton<IEventBus, RabbitMqEventBus>();
}
else if (messagingProvider == "AzureServiceBus")
{
    builder.Services.AddSingleton<ServiceBusClient>(sp =>
        new ServiceBusClient(
            builder.Configuration["Messaging:AzureServiceBus:ConnectionString"]
        ));

    services.AddSingleton<IEventBus, AzureServiceBusEventBus>();
}
else
{
    bootstrapLogger.Error("Unsupported Messaging Provider: {Provider}. Messaging is disabled.", messagingProvider);
    services.AddSingleton<IEventBus, NoOpEventBus>();
}

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            new string[]{}
        }
    });
});

// Build and run
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

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await RoleSeeder.SeedAsync(roleManager);
}

app.Run();
