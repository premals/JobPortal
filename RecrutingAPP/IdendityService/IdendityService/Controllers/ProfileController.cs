using IdendityService.DTOs;
using IdendityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdendityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ProfileController(UserManager<ApplicationUser> userManager) => _userManager = userManager;

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (userId == null) return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new
            {
                user.FullName,
                user.Email,
                UserType = roles.FirstOrDefault() ?? "JobSeeker"
            });
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateProfileRequest model)
        {
            var userId = User.FindFirst("userId")?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();
            user.FullName = model.FullName;
            await _userManager.UpdateAsync(user);
            return Ok();
        }
    }
}
