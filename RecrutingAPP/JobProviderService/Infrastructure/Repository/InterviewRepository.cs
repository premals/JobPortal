using JobProviderService.Application.Interfaces;
using JobProviderService.Domain;
using MongoDB.Driver;

namespace JobProviderService.Infrastructure.Repository
{
    public class InterviewRepository : IInterviewRepository
    {
        private readonly IMongoCollection<InterviewInvite> _invites;
        private readonly IMongoCollection<InterviewSession> _sessions;

        public InterviewRepository(MongoDbContext context)
        {
            _invites = context.InterviewInvites;
            _sessions = context.InterviewSessions;
        }

        public async Task CreateInviteAsync(InterviewInvite invite)
        {
            ArgumentNullException.ThrowIfNull(invite);
            await _invites.InsertOneAsync(invite);
        }

        public async Task<InterviewInvite?> GetInviteAsync(string inviteId)
        {
            if (string.IsNullOrWhiteSpace(inviteId))
                return null;

            return await _invites.Find(x => x.Id == inviteId).FirstOrDefaultAsync();
        }

        public async Task<List<InterviewInvite>> GetInvitesByProviderAsync(string providerId)
        {
            if (string.IsNullOrWhiteSpace(providerId))
                return new List<InterviewInvite>();

            return await _invites
                .Find(x => x.JobProviderId == providerId)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<InterviewInvite>> GetInvitesBySeekerAsync(string seekerId)
        {
            if (string.IsNullOrWhiteSpace(seekerId))
                return new List<InterviewInvite>();

            return await _invites
                .Find(x => x.JobSeekerId == seekerId)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateInviteAsync(InterviewInvite invite)
        {
            ArgumentNullException.ThrowIfNull(invite);
            invite.UpdatedAt = DateTime.UtcNow;

            await _invites.ReplaceOneAsync(x => x.Id == invite.Id, invite);
        }

        public async Task CreateSessionAsync(InterviewSession session)
        {
            ArgumentNullException.ThrowIfNull(session);
            await _sessions.InsertOneAsync(session);
        }

        public async Task<InterviewSession?> GetSessionAsync(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return null;

            return await _sessions.Find(x => x.Id == sessionId).FirstOrDefaultAsync();
        }

        public async Task<List<InterviewSession>> GetSessionsByProviderAsync(string providerId)
        {
            if (string.IsNullOrWhiteSpace(providerId))
                return new List<InterviewSession>();

            return await _sessions
                .Find(x => x.JobProviderId == providerId)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<InterviewSession>> GetSessionsBySeekerAsync(string seekerId)
        {
            if (string.IsNullOrWhiteSpace(seekerId))
                return new List<InterviewSession>();

            return await _sessions
                .Find(x => x.JobSeekerId == seekerId)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<InterviewSession?> GetLatestSessionAsync(string providerId, string jobId, string jobSeekerId)
        {
            if (string.IsNullOrWhiteSpace(providerId)
                || string.IsNullOrWhiteSpace(jobId)
                || string.IsNullOrWhiteSpace(jobSeekerId))
                return null;

            return await _sessions
                .Find(x => x.JobProviderId == providerId
                           && x.JobId == jobId
                           && x.JobSeekerId == jobSeekerId)
                .SortByDescending(x => x.UpdatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateSessionAsync(InterviewSession session)
        {
            ArgumentNullException.ThrowIfNull(session);
            session.UpdatedAt = DateTime.UtcNow;

            await _sessions.ReplaceOneAsync(x => x.Id == session.Id, session);
        }
    }
}
