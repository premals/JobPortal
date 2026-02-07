using Gateway.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Config;
using NLog.Web;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Diagnostics;

LogManager.Setup().SetupExtensions(ext =>
    ext.RegisterTarget<AzureLogAnalyticsTarget>("AzureLogAnalytics"));

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseNLog();

Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "logs"));

// Load Ocelot configuration (ocelot.json)
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Gateway",
        Version = "v1"
    });

    // JWT Support
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});



builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = false,
            ValidateLifetime = true
        };
    });

// Register Ocelot
builder.Services.AddOcelot(builder.Configuration);

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

app.UseCors("GatewaySwaggerCors");
app.UseCors("AllowAngular");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway");

    c.SwaggerEndpoint("http://localhost:5133/swagger/v1/swagger.json", "Job Provider Service");
    c.SwaggerEndpoint("http://localhost:7210//swagger/v1/swagger.json", "Job Seeker Service");
    c.SwaggerEndpoint("http://localhost:5198/swagger/v1/swagger.json", "Identity Service");
});

await app.UseOcelot();



app.Run();
