using JobSeekerService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobSeekerService.Controllers
{
    [ApiController]
    [Route("api/jobseeker/notifications")]
    [Authorize(Policy = "RequireJobSeeker")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _repository;

        public NotificationsController(INotificationRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int limit = 50)
        {
            var seekerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(seekerId))
                return Unauthorized();

            var notifications = await _repository.GetBySeekerAsync(seekerId, limit);
            return Ok(notifications);
        }

        [HttpPatch("{notificationId}/read")]
        public async Task<IActionResult> MarkRead(string notificationId)
        {
            var seekerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(seekerId))
                return Unauthorized();

            await _repository.MarkReadAsync(notificationId, seekerId);
            return Ok(new { message = "Notification marked as read." });
        }
    }
}
