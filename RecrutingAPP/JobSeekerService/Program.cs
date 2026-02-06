using JobSeekerService.Application.EventHandler;
using JobSeekerService.Application.Interfaces;
using JobSeekerService.Application.UseCases;
using JobSeekerService.Infrastructure.AI;
using JobSeekerService.Infrastructure.Messaging;
using JobSeekerService.Infrastructure.Messaging.AzureServiceBus;
using JobSeekerService.Infrastructure.Messaging.RabbitMq;
using JobSeekerService.Infrastructure.Mongo;
using JobSeekerService.Infrastructure.Mongo.Indexes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<GenerateResumeAiUseCase>();
// ================================
// AI Services
// ================================
builder.Services.AddScoped<IResumeAiService, ResumeAiService>();

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


if (builder.Configuration["Messaging:Provider"] == "RabbitMQ")
{
    builder.Services.AddHostedService<JobEventsRabbitConsumer>();
}
else
{
    builder.Services.AddHostedService<JobEventsServiceBusConsumer>();
}

if (builder.Configuration["Messaging:Provider"] == "RabbitMQ")
{
    builder.Services.AddSingleton<IEventBus, RabbitMqEventBus>();
}
else
{
    builder.Services.AddSingleton<IEventBus, AzureServiceBusEventBus>();
}

builder.Services.AddScoped<JobSeekerRegisteredEventHandler>();
builder.Services.AddScoped<JobApplicationStatusUpdatedEventHandler>();
builder.Services.AddScoped<InterviewInviteCreatedEventHandler>();

if (builder.Configuration["Messaging:Provider"] == "RabbitMQ")
{
    builder.Services.AddHostedService<JobSeekerRegisteredConsumer>();
}
else
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
using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

    await new JobReadIndexSeeder(database).CreateIndexesAsync();
    await new JobSeekerIndexSeeder(database).CreateIndexesAsync();
    await new JobApplicationIndexSeeder(database).CreateIndexesAsync();
    await new NotificationIndexSeeder(database).CreateIndexesAsync();
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







