using IdendityService.DTOs;
using IdendityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace IdendityService.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Policy = "RequireAdminRole")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var usersQuery = _userManager.Users.AsQueryable();

            var users = await usersQuery.ToListAsync();

            var jobSeekers = 0;
            var jobProviders = 0;
            var admins = 0;

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin")) admins++;
                if (roles.Contains("JobProvider")) jobProviders++;
                if (roles.Contains("JobSeeker")) jobSeekers++;
            }

            return Ok(new
            {
                totalUsers = users.Count,
                jobSeekers,
                jobProviders,
                admins
            });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? role)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            var users = await usersQuery.ToListAsync();
            var result = new List<AdminUserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var primaryRole = roles.FirstOrDefault() ?? "JobSeeker";

                if (!string.IsNullOrWhiteSpace(role) && !roles.Contains(role))
                    continue;

                result.Add(new AdminUserDto
                {
                    UserId = user.Id.ToString(),
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Role = primaryRole,
                    IsActive = user.IsActive,
                    ForcePasswordReset = user.ForcePasswordReset
                });
            }

            return Ok(result);
        }

        [HttpPut("users/{userId}/status")]
        public async Task<IActionResult> UpdateStatus(string userId, [FromBody] UpdateUserStatusRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.IsActive = request.IsActive;
            await _userManager.UpdateAsync(user);
            return Ok();
        }

        [HttpPut("users/{userId}/role")]
        public async Task<IActionResult> UpdateRole(string userId, [FromBody] UpdateUserRoleRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (!await _roleManager.RoleExistsAsync(request.Role))
                return BadRequest("Role does not exist.");

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Count > 0)
                await _userManager.RemoveFromRolesAsync(user, roles);

            await _userManager.AddToRoleAsync(user, request.Role);
            return Ok();
        }

        [HttpPost("users/{userId}/reset")]
        public async Task<IActionResult> ResetUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.RefreshTokens.Clear();
            user.ForcePasswordReset = true;
            await _userManager.UpdateAsync(user);
            return Ok();
        }
    }
}
