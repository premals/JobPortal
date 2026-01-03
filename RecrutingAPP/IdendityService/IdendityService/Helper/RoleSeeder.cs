using IdendityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdendityService.Helper
{
    public class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager)
        {
            var roles = new[] { "JobProvider", "JobSeeker", "Admin" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = role });
                }
            }
        }
    }
}
