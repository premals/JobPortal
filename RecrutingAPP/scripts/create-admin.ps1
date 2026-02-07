param(
    [Parameter(Mandatory = $true)]
    [string]$Email,

    [Parameter(Mandatory = $true)]
    [string]$Password,

    [Parameter(Mandatory = $true)]
    [string]$FullName,

    [string]$MongoConnectionString,
    [string]$MongoDatabase,

    [switch]$ForcePasswordReset,
    [switch]$ResetPassword,
    [switch]$KeepTemp
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$appSettingsPath = Join-Path $repoRoot "IdendityService/IdendityService/appsettings.json"

if ((-not $MongoConnectionString) -or (-not $MongoDatabase)) {
    if (Test-Path $appSettingsPath) {
        $config = Get-Content -Raw -Path $appSettingsPath | ConvertFrom-Json
        if (-not $MongoConnectionString) { $MongoConnectionString = $config.MongoSettings.ConnectionString }
        if (-not $MongoDatabase) { $MongoDatabase = $config.MongoSettings.DatabaseName }
    }
}

if (-not $MongoConnectionString -or -not $MongoDatabase) {
    throw "Mongo settings are missing. Provide -MongoConnectionString and -MongoDatabase or ensure appsettings.json has MongoSettings."
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("admin-seed-" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $tempRoot | Out-Null

$csproj = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <!-- Gives access to Microsoft.AspNetCore.Identity types without old NuGet packages -->
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="AspNetCore.Identity.MongoDbCore" Version="2.0.7" />
  </ItemGroup>
</Project>
'@
$program = @'
using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[CollectionName("Users")]
public class ApplicationUser : MongoIdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public bool EmailVerified { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool ForcePasswordReset { get; set; } = false;
}

[CollectionName("Roles")]
public class ApplicationRole : MongoIdentityRole<Guid> { }

static string? GetArg(string name, string[] args)
{
    var index = Array.IndexOf(args, name);
    return (index >= 0 && index + 1 < args.Length) ? args[index + 1] : null;
}

static bool HasFlag(string name, string[] args) => args.Contains(name);

var email = GetArg("--email", args);
var password = GetArg("--password", args);
var fullName = GetArg("--full-name", args);
var mongo = GetArg("--mongo", args);
var db = GetArg("--db", args);
var forceReset = HasFlag("--force-reset", args);
var resetPassword = HasFlag("--reset-password", args);

if (string.IsNullOrWhiteSpace(email) ||
    string.IsNullOrWhiteSpace(password) ||
    string.IsNullOrWhiteSpace(fullName) ||
    string.IsNullOrWhiteSpace(mongo) ||
    string.IsNullOrWhiteSpace(db))
{
    Console.Error.WriteLine("Missing required arguments.");
    Console.Error.WriteLine("Required: --email --password --full-name --mongo --db");
    Environment.Exit(1);
}

var builder = Host.CreateApplicationBuilder();
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(mongo, db)
    .AddDefaultTokenProviders();

using var host = builder.Build();
using var scope = host.Services.CreateScope();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

if (!await roleManager.RoleExistsAsync("Admin"))
{
    await roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });
}

var user = await userManager.FindByEmailAsync(email);
if (user == null)
{
    user = new ApplicationUser
    {
        UserName = email,
        Email = email,
        FullName = fullName,
        EmailConfirmed = true,
        EmailVerified = true,
        IsActive = true,
        ForcePasswordReset = forceReset
    };

    var create = await userManager.CreateAsync(user, password);
    if (!create.Succeeded)
    {
        Console.Error.WriteLine("Failed to create user: " + string.Join("; ", create.Errors.Select(e => e.Description)));
        Environment.Exit(2);
    }
}
else
{
    user.FullName = fullName;
    user.EmailConfirmed = true;
    user.EmailVerified = true;
    user.IsActive = true;
    if (forceReset) user.ForcePasswordReset = true;

    var update = await userManager.UpdateAsync(user);
    if (!update.Succeeded)
    {
        Console.Error.WriteLine("Failed to update user: " + string.Join("; ", update.Errors.Select(e => e.Description)));
        Environment.Exit(3);
    }

    if (resetPassword)
    {
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var reset = await userManager.ResetPasswordAsync(user, token, password);
        if (!reset.Succeeded)
        {
            Console.Error.WriteLine("Failed to reset password: " + string.Join("; ", reset.Errors.Select(e => e.Description)));
            Environment.Exit(4);
        }
    }
}

if (!await userManager.IsInRoleAsync(user, "Admin"))
{
    var addRole = await userManager.AddToRoleAsync(user, "Admin");
    if (!addRole.Succeeded)
    {
        Console.Error.WriteLine("Failed to add Admin role: " + string.Join("; ", addRole.Errors.Select(e => e.Description)));
        Environment.Exit(5);
    }
}

Console.WriteLine($"Admin user ensured: {email}");
'@

Set-Content -Path (Join-Path $tempRoot "AdminSeeder.csproj") -Value $csproj -Encoding utf8
Set-Content -Path (Join-Path $tempRoot "Program.cs") -Value $program -Encoding utf8

$dotnetArgs = @(
    "run",
    "--project", $tempRoot,
    "--",
    "--email", $Email,
    "--password", $Password,
    "--full-name", $FullName,
    "--mongo", $MongoConnectionString,
    "--db", $MongoDatabase
)

if ($ForcePasswordReset) { $dotnetArgs += "--force-reset" }
if ($ResetPassword) { $dotnetArgs += "--reset-password" }

& dotnet @dotnetArgs
$exitCode = $LASTEXITCODE

if (-not $KeepTemp) {
    Remove-Item -Recurse -Force $tempRoot
}

if ($exitCode -ne 0) {
    throw "Admin seeding failed. Exit code: $exitCode"
}
