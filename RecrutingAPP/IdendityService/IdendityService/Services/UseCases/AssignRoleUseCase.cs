using IdendityService.Interfaces.Auth;
using IdendityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdendityService.Services.UseCases
{
    public class AssignRoleUseCase : IAssignRoleUseCase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AssignRoleUseCase(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task AssignAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ApplicationException("User not found");

            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
                throw new ApplicationException("Role assignment failed");
        }
    }
}
