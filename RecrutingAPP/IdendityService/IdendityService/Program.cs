using AspNetCore.Identity.MongoDbCore.Extensions;
using Azure.Messaging.ServiceBus;
using IdendityService.Helper;
using IdendityService.Infrastructure.Messaging;
using IdendityService.Infrastructure.Messaging.AzureServiceBus;
using IdendityService.Infrastructure.Messaging.RabbitMq;
using IdendityService.Interfaces;
using IdendityService.Interfaces.Auth;
using IdendityService.Models;
using IdendityService.Services;
using IdendityService.Services.UseCases;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

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
    throw new InvalidOperationException(
        $"Unsupported Messaging Provider: {messagingProvider}");
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
