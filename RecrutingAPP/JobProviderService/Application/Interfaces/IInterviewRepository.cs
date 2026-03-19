using JobProviderService.Domain;

namespace JobProviderService.Application.Interfaces
{
    public interface IInterviewRepository
    {
        Task CreateInviteAsync(InterviewInvite invite);
        Task<InterviewInvite?> GetInviteAsync(string inviteId);
        Task<InterviewInvite?> GetInviteByTokenAsync(string token);
        Task<List<InterviewInvite>> GetInvitesByProviderAsync(string providerId);
        Task<List<InterviewInvite>> GetInvitesBySeekerAsync(string seekerId);
        Task UpdateInviteAsync(InterviewInvite invite);

        Task CreateSessionAsync(InterviewSession session);
        Task<InterviewSession?> GetSessionAsync(string sessionId);
        Task<InterviewSession?> GetSessionByInviteIdAsync(string inviteId);
        Task<List<InterviewSession>> GetSessionsByProviderAsync(string providerId);
        Task<List<InterviewSession>> GetSessionsBySeekerAsync(string seekerId);
        Task<InterviewSession?> GetLatestSessionAsync(string providerId, string jobId, string jobSeekerId);
        Task UpdateSessionAsync(InterviewSession session);
    }
}
