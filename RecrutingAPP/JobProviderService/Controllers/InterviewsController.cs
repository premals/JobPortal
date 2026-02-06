using JobProviderService.Application.Interfaces;
using JobProviderService.Application.UseCases;
using JobProviderService.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;

namespace JobProviderService.Controllers
{
    [ApiController]
    [Route("api/jobs/interviews")]
    public class InterviewsController : ControllerBase
    {
        private readonly IInterviewRepository _interviews;
        private readonly InterviewSessionUseCase _sessionUseCase;

        public InterviewsController(
            IInterviewRepository interviews,
            InterviewSessionUseCase sessionUseCase)
        {
            _interviews = interviews;
            _sessionUseCase = sessionUseCase;
        }

        [HttpGet("invites")]
        [Authorize(Policy = "RequireJobSeeker")]
        public async Task<IActionResult> MyInvites()
        {
            var seekerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(seekerId))
                return Unauthorized();

            var invites = await _interviews.GetInvitesBySeekerAsync(seekerId);
            return Ok(invites);
        }

        [HttpPost("invites/{inviteId}/accept")]
        [Authorize(Policy = "RequireJobSeeker")]
        public async Task<IActionResult> AcceptInvite(
            string inviteId,
            [FromBody] InterviewInviteAcceptRequest request)
        {
            var seekerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(seekerId))
                return Unauthorized();

            var session = await _sessionUseCase.AcceptInviteAsync(inviteId, seekerId, request.SelectedSlot);
            return Ok(session);
        }

        [HttpPost("{sessionId}/start")]
        [Authorize(Policy = "RequireJobSeeker")]
        public async Task<IActionResult> StartSession(string sessionId)
        {
            var seekerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(seekerId))
                return Unauthorized();

            var question = await _sessionUseCase.StartSessionAsync(sessionId, seekerId);
            var session = await _interviews.GetSessionAsync(sessionId);
            return Ok(new InterviewStartResponse
            {
                SessionId = sessionId,
                Question = question,
                QuestionIndex = 1,
                TotalQuestions = session?.TotalQuestions ?? 0
            });
        }

        [HttpPost("{sessionId}/answer")]
        [Authorize(Policy = "RequireJobSeeker")]
        public async Task<IActionResult> Answer(string sessionId, [FromBody] InterviewAnswerRequest request)
        {
            var seekerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(seekerId))
                return Unauthorized();

            var session = await _sessionUseCase.AnswerAsync(sessionId, seekerId, request.Answer);

            var nextIndex = session.CurrentQuestionIndex + 1;
            var nextQuestion = session.Status == "Completed"
                ? null
                : session.Questions.ElementAtOrDefault(session.CurrentQuestionIndex);

            return Ok(new InterviewAnswerResponse
            {
                Completed = session.Status == "Completed",
                NextQuestion = nextQuestion,
                QuestionIndex = nextIndex,
                TotalQuestions = session.TotalQuestions,
                Evaluation = session.Evaluation
            });
        }

        [HttpGet("{sessionId}")]
        [Authorize(Policy = "RequireJobProvider")]
        public async Task<IActionResult> GetSession(string sessionId)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            var session = await _interviews.GetSessionAsync(sessionId);
            if (session == null || session.JobProviderId != providerId)
                return NotFound();

            return Ok(session);
        }

        [HttpGet("provider/invites")]
        [Authorize(Policy = "RequireJobProvider")]
        public async Task<IActionResult> GetProviderInvites([FromQuery] string? from, [FromQuery] string? to)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            var invites = await _interviews.GetInvitesByProviderAsync(providerId);
            var scheduled = invites
                .Where(x => x.Status == "Accepted" && x.SelectedSlot.HasValue)
                .OrderBy(x => x.SelectedSlot);

            if (DateTime.TryParse(from, out var fromDate))
                scheduled = scheduled.Where(x => x.SelectedSlot >= fromDate).OrderBy(x => x.SelectedSlot);

            if (DateTime.TryParse(to, out var toDate))
                scheduled = scheduled.Where(x => x.SelectedSlot <= toDate).OrderBy(x => x.SelectedSlot);

            return Ok(scheduled);
        }

        [HttpGet("report")]
        [Authorize(Policy = "RequireJobProvider")]
        public async Task<IActionResult> GetInterviewReport([FromQuery] string jobId, [FromQuery] string jobSeekerId)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(jobId) || string.IsNullOrWhiteSpace(jobSeekerId))
                return BadRequest("jobId and jobSeekerId are required.");

            var session = await _interviews.GetLatestSessionAsync(providerId, jobId, jobSeekerId);
            if (session == null)
                return NotFound();

            return Ok(new InterviewReportResponse
            {
                SessionId = session.Id,
                JobId = session.JobId,
                JobSeekerId = session.JobSeekerId,
                Status = session.Status,
                Difficulty = session.Difficulty,
                Skills = session.Skills,
                TotalQuestions = session.TotalQuestions,
                UpdatedAt = session.UpdatedAt,
                Evaluation = session.Evaluation
            });
        }
    }
}
