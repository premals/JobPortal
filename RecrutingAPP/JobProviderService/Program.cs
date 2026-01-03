using Azure.Messaging.ServiceBus;
using JobProviderService.Application;
using JobProviderService.Application.EventHandlers;
using JobProviderService.Application.Interfaces;
using JobProviderService.Application.UseCases;
using JobProviderService.Domain;
using JobProviderService.Infrastructure;
using JobProviderService.Infrastructure.Indexes;
using JobProviderService.Infrastructure.Messaging;
using JobProviderService.Infrastructure.Messaging.Azure;
using JobProviderService.Infrastructure.Messaging.RabbitMQ;
using JobProviderService.Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSingleton<MongoDbContext>();

// =======================
// Dependency Injection
// =======================
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
builder.Services.AddScoped<CreateJobUseCase>();
builder.Services.AddScoped<UpdateJobUseCase>();
builder.Services.AddScoped<DeleteJobUseCase>();
builder.Services.AddScoped<UpdateJobPartialUseCase>();
builder.Services.AddScoped<JobAppliedEventHandler>();
builder.Services.AddScoped<JobApplicationWithdrawnEventHandler>();

// -----------------------------
// Messaging Configuration
// -----------------------------
var messagingProvider = builder.Configuration["Messaging:Provider"];

if (messagingProvider == "RabbitMQ")
{
    builder.Services.AddSingleton<IEventBus, RabbitMqEventBus>();
}
else if (messagingProvider == "AzureServiceBus")
{
    builder.Services.AddSingleton<ServiceBusClient>(sp =>
        new ServiceBusClient(
            builder.Configuration["Messaging:AzureServiceBus:ConnectionString"]
        ));

    builder.Services.AddSingleton<IEventBus, AzureServiceBusEventBus>();
}
else
{
    throw new InvalidOperationException(
        $"Unsupported Messaging Provider: {messagingProvider}");
}

if (builder.Configuration["Messaging:Provider"] == "RabbitMQ")
{
    builder.Services.AddHostedService<JobApplicationConsumer>();
}
else
{
    builder.Services.AddHostedService<JobApplicationServiceBusConsumer>();
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
            Console.WriteLine(context.Exception.Message);
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
