using JobProviderService.Application.Interfaces;
using JobProviderService.Application.UseCases;
using JobProviderService.Domain;
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
        private readonly IInterviewRecordingStorage _recordingStorage;

        public InterviewsController(
            IInterviewRepository interviews,
            InterviewSessionUseCase sessionUseCase,
            IInterviewRecordingStorage recordingStorage)
        {
            _interviews = interviews;
            _sessionUseCase = sessionUseCase;
            _recordingStorage = recordingStorage;
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

        [HttpGet("public/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicInvite(string token)
        {
            var invite = await _interviews.GetInviteByTokenAsync(token);
            if (invite == null)
                return NotFound();

            if (invite.TokenExpiresAt.HasValue && invite.TokenExpiresAt.Value < DateTime.UtcNow)
                return BadRequest("Invite link expired.");

            if (invite.TokenUsedAt.HasValue)
                return BadRequest("Invite link already used.");

            var session = await _interviews.GetSessionByInviteIdAsync(invite.Id);

            return Ok(new PublicInterviewInviteResponse
            {
                InviteId = invite.Id,
                JobTitle = invite.JobTitle,
                CandidateName = invite.CandidateName,
                CandidateEmail = invite.CandidateEmail,
                Status = invite.Status,
                Difficulty = invite.Difficulty,
                QuestionsCount = invite.QuestionsCount,
                ProposedSlots = invite.ProposedSlots,
                SelectedSlot = invite.SelectedSlot,
                TokenExpiresAt = invite.TokenExpiresAt,
                SessionId = session?.Id
            });
        }

        [HttpPost("public/{token}/accept")]
        [AllowAnonymous]
        public async Task<IActionResult> AcceptPublicInvite(
            string token,
            [FromBody] InterviewInviteAcceptRequest request)
        {
            if (request == null || request.SelectedSlot == default)
                return BadRequest("SelectedSlot is required.");

            if (request.SelectedSlot <= DateTime.UtcNow)
                return BadRequest("Selected slot must be in the future.");

            var session = await _sessionUseCase.AcceptInviteByTokenAsync(token, request.SelectedSlot);
            return Ok(new PublicInterviewSessionResponse
            {
                SessionId = session.Id,
                Status = session.Status,
                ScheduledStart = session.ScheduledStart,
                ScheduledEnd = session.ScheduledEnd,
                Questions = session.Questions,
                AvatarProvider = session.AvatarProvider
            });
        }

        [HttpGet("public/{token}/session")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicSession(string token)
        {
            var invite = await _interviews.GetInviteByTokenAsync(token);
            if (invite == null)
                return NotFound();

            if (invite.TokenExpiresAt.HasValue && invite.TokenExpiresAt.Value < DateTime.UtcNow)
                return BadRequest("Invite link expired.");

            if (invite.TokenUsedAt.HasValue)
                return BadRequest("Invite link already used.");

            var session = await _interviews.GetSessionByInviteIdAsync(invite.Id);
            if (session == null)
                return NotFound();

            return Ok(new PublicInterviewSessionResponse
            {
                SessionId = session.Id,
                Status = session.Status,
                ScheduledStart = session.ScheduledStart,
                ScheduledEnd = session.ScheduledEnd,
                Questions = session.Questions,
                AvatarProvider = session.AvatarProvider
            });
        }

        [HttpPost("public/{token}/start")]
        [AllowAnonymous]
        public async Task<IActionResult> StartPublicSession(string token)
        {
            var invite = await _interviews.GetInviteByTokenAsync(token);
            if (invite == null)
                return NotFound();

            if (invite.TokenExpiresAt.HasValue && invite.TokenExpiresAt.Value < DateTime.UtcNow)
                return BadRequest("Invite link expired.");

            if (invite.TokenUsedAt.HasValue)
                return BadRequest("Invite link already used.");

            var session = await _interviews.GetSessionByInviteIdAsync(invite.Id);
            if (session == null)
                return NotFound();

            var access = await EnsurePublicAccessWindowAsync(invite, session);
            if (!access.Allowed)
                return BadRequest(access.Message);

            var question = await _sessionUseCase.StartSessionAsync(session.Id, invite.JobSeekerId);
            return Ok(new InterviewStartResponse
            {
                SessionId = session.Id,
                Question = question,
                QuestionIndex = 1,
                TotalQuestions = session.TotalQuestions
            });
        }

        [HttpPost("public/{token}/upload")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadPublicResponse(
            string token,
            [FromForm] PublicInterviewUploadRequest request)
        {
            if (request == null || request.Video == null || request.Video.Length == 0)
                return BadRequest("Video file is required.");

            if (string.IsNullOrWhiteSpace(request.AnswerText))
                return BadRequest("AnswerText is required.");

            var invite = await _interviews.GetInviteByTokenAsync(token);
            if (invite == null)
                return NotFound();

            if (invite.TokenExpiresAt.HasValue && invite.TokenExpiresAt.Value < DateTime.UtcNow)
                return BadRequest("Invite link expired.");

            if (invite.TokenUsedAt.HasValue)
                return BadRequest("Invite link already used.");

            var session = await _interviews.GetSessionByInviteIdAsync(invite.Id);
            if (session == null)
                return NotFound();

            var access = await EnsurePublicAccessWindowAsync(invite, session);
            if (!access.Allowed)
                return BadRequest(access.Message);

            if (session.Status == "Scheduled")
            {
                await _sessionUseCase.StartSessionAsync(session.Id, invite.JobSeekerId);
            }

            var updated = await _sessionUseCase.AnswerAsync(session.Id, invite.JobSeekerId, request.AnswerText);

            var questionIndex = request.QuestionIndex > 0 ? request.QuestionIndex : updated.CurrentQuestionIndex;
            var questionText = updated.Questions.ElementAtOrDefault(questionIndex - 1) ?? string.Empty;

            var ext = Path.GetExtension(request.Video.FileName);
            var safeExt = string.IsNullOrWhiteSpace(ext) ? ".webm" : ext;
            var blobPath = $"sessions/{updated.Id}/q{questionIndex}-{Guid.NewGuid():N}{safeExt}";

            await using var stream = request.Video.OpenReadStream();
            var videoUrl = await _recordingStorage.UploadAsync(
                blobPath,
                stream,
                request.Video.ContentType,
                HttpContext.RequestAborted);

            updated.Responses.RemoveAll(r => r.QuestionIndex == questionIndex);
            updated.Responses.Add(new InterviewResponse
            {
                QuestionIndex = questionIndex,
                Question = questionText,
                AnswerText = request.AnswerText,
                VideoUrl = videoUrl,
                DurationSeconds = request.DurationSeconds,
                RecordedAt = DateTime.UtcNow
            });

            await _interviews.UpdateSessionAsync(updated);

            return Ok(new
            {
                sessionId = updated.Id,
                status = updated.Status,
                totalQuestions = updated.TotalQuestions,
                currentQuestionIndex = updated.CurrentQuestionIndex,
                evaluation = updated.Evaluation
            });
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
            await UpdateNoShowsAsync(invites);
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

        private async Task<(bool Allowed, string Message)> EnsurePublicAccessWindowAsync(
            InterviewInvite invite,
            InterviewSession session)
        {
            if (!string.Equals(session.Status, "Scheduled", StringComparison.OrdinalIgnoreCase))
                return (true, string.Empty);

            if (!session.ScheduledStart.HasValue)
                return (true, string.Empty);

            var now = DateTime.UtcNow;
            var start = session.ScheduledStart.Value;
            var end = session.ScheduledEnd ?? start;

            if (now < start)
                return (false, "Interview is not available yet. Please join at the scheduled time.");

            if (now > end)
            {
                await _sessionUseCase.MarkNoShowAsync(invite, session);
                return (false, "Interview window has ended. Please contact your recruiter for a new invite.");
            }

            return (true, string.Empty);
        }

        private async Task UpdateNoShowsAsync(IEnumerable<InterviewInvite> invites)
        {
            var now = DateTime.UtcNow;
            foreach (var invite in invites)
            {
                if (!invite.SelectedSlot.HasValue)
                    continue;

                if (!string.Equals(invite.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
                    continue;

                var session = await _interviews.GetSessionByInviteIdAsync(invite.Id);
                if (session == null)
                    continue;

                if (!string.Equals(session.Status, "Scheduled", StringComparison.OrdinalIgnoreCase))
                    continue;

                var end = session.ScheduledEnd ?? session.ScheduledStart;
                if (!end.HasValue || now <= end.Value)
                    continue;

                await _sessionUseCase.MarkNoShowAsync(invite, session);
            }
        }
    }
}
