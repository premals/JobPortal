using IdendityService.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace IdendityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRegisterUserUseCase _register;
        private readonly ILoginUseCase _login;
        private readonly IRefreshTokenUseCase _refresh;
        private readonly IForgot<secret>UseCase _forgot;
        private readonly IReset<secret>UseCase _reset;
        private readonly IConfirmEmailUseCase _confirm;
        private readonly IAssignRoleUseCase _assignRole;
        private readonly IConfiguration _configuration;

        public AuthController(
            IRegisterUserUseCase register,
            ILoginUseCase login,
            IRefreshTokenUseCase refresh,
            IForgot<secret>UseCase forgot,
            IReset<secret>UseCase reset,
            IConfirmEmailUseCase confirm,
            IAssignRoleUseCase assignRole,
            IConfiguration configuration)
        {
            _register = register;
            _login = login;
            _refresh = refresh;
            _forgot = forgot;
            _reset = reset;
            _confirm = confirm;
            _assignRole = assignRole;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] IdendityService.DTOs.RegisterRequest req)
        {
            var confirmBaseUrl = _configuration["AppUrls:IdentityBaseUrl"];
            var confirmPath = ResolveConfirmEmailPath(confirmBaseUrl, _configuration["AppUrls:ConfirmEmailPath"]);
            var url = string.IsNullOrWhiteSpace(confirmBaseUrl)
                ? $"{Request.Scheme}://{Request.Host}{confirmPath}"
                : $"{confirmBaseUrl.TrimEnd('/')}{confirmPath}";
            var result = await _register.RegisterAsync(req, url);
            return result.Success == true ? Ok(result) : BadRequest(result.Message);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            await _confirm.ConfirmAsync(userId, token);

            var uiBaseUrl = _configuration["AppUrls:FrontendBaseUrl"];
            if (string.IsNullOrWhiteSpace(uiBaseUrl))
            {
                return Ok();
            }

            var loginPath = _configuration["AppUrls:LoginPath"] ?? "/login";
            var redirectUrl = $"{uiBaseUrl.TrimEnd('/')}{loginPath}?verified=1";
            return Redirect(redirectUrl);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] IdendityService.DTOs.LoginRequest req)
            => Ok(await _login.LoginAsync(req, HttpContext.Connection.RemoteIpAddress?.ToString()));

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string token)
            => Ok(await _refresh.RefreshAsync(token, HttpContext.Connection.RemoteIpAddress?.ToString()));

        [HttpPost("forgot-<secret>")]
        public async Task<IActionResult> Forgot(IdendityService.DTOs.Forgot<secret>RequestDto.Forgot<secret>Request req)
        {
            var uiBaseUrl = _configuration["AppUrls:FrontendBaseUrl"];
            var resetPath = _configuration["AppUrls:Reset<secret>Path"] ?? "/reset-<secret>";

            var url = string.IsNullOrWhiteSpace(uiBaseUrl)
                ? $"{Request.Scheme}://{Request.Host}/api/auth/reset-<secret>"
                : $"{uiBaseUrl.TrimEnd('/')}{resetPath}";

            await _forgot.ExecuteAsync(req.Email, url);
            return Ok();
        }

        [HttpPost("reset-<secret>")]
        public async Task<IActionResult> Reset([FromBody] IdendityService.DTOs.Reset<secret>RequestDto.Reset<secret>Request req)
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

        private static string ResolveConfirmEmailPath(string? confirmBaseUrl, string? configuredPath)
        {
            var path = string.IsNullOrWhiteSpace(configuredPath)
                ? "/api/auth/confirm-email"
                : configuredPath.Trim();

            if (!path.StartsWith('/'))
            {
                path = "/" + path;
            }

            var usesIdentityGatewayPrefix = !string.IsNullOrWhiteSpace(confirmBaseUrl)
                && confirmBaseUrl.Contains("/identity", StringComparison.OrdinalIgnoreCase);

            if (usesIdentityGatewayPrefix && path.StartsWith("/api/auth/", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring("/api/auth".Length);
            }

            return path;
        }

    }
}
