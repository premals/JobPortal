using IdendityService.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdendityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRegisterUserUseCase _register;
        private readonly ILoginUseCase _login;
        private readonly IRefreshTokenUseCase _refresh;
        private readonly IForgotPasswordUseCase _forgot;
        private readonly IResetPasswordUseCase _reset;
        private readonly IConfirmEmailUseCase _confirm;
        private readonly IAssignRoleUseCase _assignRole;

        public AuthController(IRegisterUserUseCase register,
        ILoginUseCase login,
        IRefreshTokenUseCase refresh,
        IForgotPasswordUseCase forgot,
        IResetPasswordUseCase reset,
        IConfirmEmailUseCase confirm,
        IAssignRoleUseCase assignRole)
        {
            _register = register;
            _login = login;
            _refresh = refresh;
            _forgot = forgot;
            _reset = reset;
            _confirm = confirm;
            _assignRole = assignRole;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] IdendityService.DTOs.RegisterRequest req)
        {
            var url = $"{Request.Scheme}://{Request.Host}/api/auth/confirm-email";
            var result = await _register.RegisterAsync(req, url);
            return result.Success == true ? Ok(result) : BadRequest(result.Message);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            await _confirm.ConfirmAsync(userId, token);
            return Ok();
        }

    
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] IdendityService.DTOs.LoginRequest req)
        => Ok(await _login.LoginAsync(req, HttpContext.Connection.RemoteIpAddress?.ToString()));

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string token)
        => Ok(await _refresh.RefreshAsync(token, HttpContext.Connection.RemoteIpAddress?.ToString()));

        [HttpPost("forgot-password")]
        public async Task<IActionResult> Forgot(IdendityService.DTOs.ForgotPasswordRequestDto.ForgotPasswordRequest req)
        {
            var url = $"{Request.Scheme}://{Request.Host}/api/auth/reset-password";
            await _forgot.ExecuteAsync(req.Email, url);
            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> Reset([FromBody] IdendityService.DTOs.ResetPasswordRequestDto.ResetPasswordRequest req)
        {
            await _reset.ExecuteAsync(req);
            return Ok();
        }

        [HttpPost("assign-role")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> AssignRole([FromQuery] string userId, [FromQuery] string role)
        {
            await _assignRole.AssignAsync(userId, role);
            return Ok();
        }

    }
}