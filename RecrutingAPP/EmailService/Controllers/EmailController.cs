using EmailService.DTOs;
using EmailService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Controllers
{
    [ApiController]
    [Route("api/email")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public EmailController(IEmailSender emailSender, IConfiguration configuration)
        {
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.To))
                return BadRequest("To is required.");

            if (string.IsNullOrWhiteSpace(request.Subject))
                return BadRequest("Subject is required.");

            if (string.IsNullOrWhiteSpace(request.Html))
                return BadRequest("Html is required.");

            var apiKey = _configuration["EmailService:ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                if (!Request.Headers.TryGetValue("X-Email-Api-Key", out var provided) ||
                    !string.Equals(provided, apiKey, StringComparison.Ordinal))
                {
                    return Unauthorized();
                }
            }

            await _emailSender.SendAsync(request.To, request.Subject, request.Html);
            return Ok();
        }
    }
}
