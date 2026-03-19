using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDbGenericRepository.Attributes;
using System.Text.Json;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var options = Args.Parse(args);
        if (!options.IsValid)
        {
            Args.PrintUsage();
            return 1;
        }

        if (string.IsNullOrWhiteSpace(options.MongoConnectionString) ||
            string.IsNullOrWhiteSpace(options.MongoDatabase))
        {
            var fromConfig = TryLoadMongoFromAppSettings(options.AppSettingsPath);
            if (fromConfig != null)
            {
                options.MongoConnectionString ??= fromConfig.Value.ConnectionString;
                options.MongoDatabase ??= fromConfig.Value.DatabaseName;
            }
        }

        if (string.IsNullOrWhiteSpace(options.MongoConnectionString) ||
            string.IsNullOrWhiteSpace(options.MongoDatabase))
        {
            Console.Error.WriteLine("Mongo settings missing. Provide --mongo and --db, or ensure appsettings.json has MongoSettings.");
            return 2;
        }

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
                options.MongoConnectionString,
                options.MongoDatabase)
            .AddDefaultTokenProviders();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });
        }

        var user = await userManager.FindByEmailAsync(options.Email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = options.Email,
                Email = options.Email,
                FullName = options.FullName,
                EmailConfirmed = true,
                EmailVerified = true,
                IsActive = true,
                Force<secret>Reset = options.Force<secret>Reset
            };

            var create = await userManager.CreateAsync(user, options.<secret>);
            if (!create.Succeeded)
            {
                Console.Error.WriteLine("Failed to create user: " + string.Join("; ", create.Errors.Select(e => e.Description)));
                return 3;
            }
        }
        else
        {
            user.FullName = options.FullName;
            user.EmailConfirmed = true;
            user.EmailVerified = true;
            user.IsActive = true;
            if (options.Force<secret>Reset) user.Force<secret>Reset = true;

            var update = await userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                Console.Error.WriteLine("Failed to update user: " + string.Join("; ", update.Errors.Select(e => e.Description)));
                return 4;
            }

            if (options.Reset<secret>)
            {
                var token = await userManager.Generate<secret>ResetTokenAsync(user);
                var reset = await userManager.Reset<secret>Async(user, token, options.<secret>);
                if (!reset.Succeeded)
                {
                    Console.Error.WriteLine("Failed to reset <secret>: " + string.Join("; ", reset.Errors.Select(e => e.Description)));
                    return 5;
                }
            }
        }

        if (!await userManager.IsInRoleAsync(user, "Admin"))
        {
            var addRole = await userManager.AddToRoleAsync(user, "Admin");
            if (!addRole.Succeeded)
            {
                Console.Error.WriteLine("Failed to add Admin role: " + string.Join("; ", addRole.Errors.Select(e => e.Description)));
                return 6;
            }
        }

        Console.WriteLine($"Admin user ensured: {options.Email}");
        return 0;
    }

    private static (string ConnectionString, string DatabaseName)? TryLoadMongoFromAppSettings(string? overridePath)
    {
        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(overridePath))
            candidates.Add(overridePath);

        candidates.Add(Path.Combine(Environment.CurrentDirectory, "IdendityService", "IdendityService", "appsettings.json"));
        candidates.Add(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "IdendityService", "IdendityService", "appsettings.json")));

        foreach (var path in candidates)
        {
            if (!File.Exists(path)) continue;

            try
            {
                using var stream = File.OpenRead(path);
                using var doc = JsonDocument.Parse(stream);
                if (doc.RootElement.TryGetProperty("MongoSettings", out var mongo))
                {
                    var cs = mongo.TryGetProperty("ConnectionString", out var csProp) ? csProp.GetString() : null;
                    var db = mongo.TryGetProperty("DatabaseName", out var dbProp) ? dbProp.GetString() : null;
                    if (!string.IsNullOrWhiteSpace(cs) && !string.IsNullOrWhiteSpace(db))
                    {
                        return (cs!, db!);
                    }
                }
            }
            catch
            {
                // Ignore and try next candidate.
            }
        }

        return null;
    }
}

[CollectionName("Users")]
public class ApplicationUser : MongoIdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public bool EmailVerified { get; set; } = true;
    public List<RefreshToken> RefreshTokens { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public bool Force<secret>Reset { get; set; } = false;
}

[CollectionName("Roles")]
public class ApplicationRole : MongoIdentityRole<Guid> { }

public class RefreshToken
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool Revoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByIp { get; set; } = string.Empty;
}

internal sealed class Args
{
    public string Email { get; private set; } = string.Empty;
    public string <secret> { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string? MongoConnectionString { get; set; }
    public string? MongoDatabase { get; set; }
    public string? AppSettingsPath { get; set; }
    public bool Force<secret>Reset { get; private set; }
    public bool Reset<secret> { get; private set; }
    public bool IsValid { get; private set; }

    public static Args Parse(string[] args)
    {
        var result = new Args();
        string? GetArg(string name)
        {
            var idx = Array.IndexOf(args, name);
            return (idx >= 0 && idx + 1 < args.Length) ? args[idx + 1] : null;
        }

        result.Email = GetArg("--email") ?? string.Empty;
        result.<secret> = GetArg("--<secret>") ?? string.Empty;
        result.FullName = GetArg("--full-name") ?? string.Empty;
        result.MongoConnectionString = GetArg("--mongo");
        result.MongoDatabase = GetArg("--db");
        result.AppSettingsPath = GetArg("--appsettings");
        result.Force<secret>Reset = args.Contains("--force-reset");
        result.Reset<secret> = args.Contains("--reset-<secret>");

        result.IsValid =
            !string.IsNullOrWhiteSpace(result.Email) &&
            !string.IsNullOrWhiteSpace(result.<secret>) &&
            !string.IsNullOrWhiteSpace(result.FullName);

        return result;
    }

    public static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project AdminSeeder -- --email <email> --<secret> <<secret>> --full-name <name>");
        Console.WriteLine("Optional:");
        Console.WriteLine("  --mongo <connectionString>   Mongo connection string");
        Console.WriteLine("  --db <databaseName>          Mongo database name");
        Console.WriteLine("  --appsettings <path>         appsettings.json path (defaults to IdentityService appsettings.json if found)");
        Console.WriteLine("  --force-reset                Set Force<secret>Reset = true");
        Console.WriteLine("  --reset-<secret>             Reset <secret> for existing user");
    }
}
