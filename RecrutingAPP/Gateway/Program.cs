using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Configuration
// --------------------------------------------------
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// --------------------------------------------------
// Swagger
// --------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Job Portal API Gateway",
        Version = "v1"
    });

    // JWT
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
            Array.Empty<string>()
        }
    });
});

// --------------------------------------------------
// CORS
// --------------------------------------------------
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

// --------------------------------------------------
// Authentication
// --------------------------------------------------
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

// --------------------------------------------------
// Ocelot
// --------------------------------------------------
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// --------------------------------------------------
// Middleware order matters
// --------------------------------------------------
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway");

    // Docker-safe endpoints
    c.SwaggerEndpoint(
        "http://host.docker.internal:5133/swagger/v1/swagger.json",
        "Job Provider Service");

    c.SwaggerEndpoint(
        "http://host.docker.internal:5025/swagger/v1/swagger.json",
        "Job Seeker Service");

    c.SwaggerEndpoint(
        "http://host.docker.internal:5198/swagger/v1/swagger.json",
        "Identity Service");
});

// MUST be last
await app.UseOcelot();

app.Run();
