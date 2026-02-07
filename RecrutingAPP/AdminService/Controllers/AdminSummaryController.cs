using AdminService.Domain.Entities;
using AdminService.Infrastructure.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/admin/summary")]
    [Authorize(Policy = "RequireAdmin")]
    public class AdminSummaryController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public AdminSummaryController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSummary()
        {
            var jobSeekerCount = await _context.JobSeekers.CountDocumentsAsync(FilterDefinition<AdminJobSeekerProjection>.Empty);
            var jobProviderCount = await _context.JobProviders.CountDocumentsAsync(FilterDefinition<AdminJobProviderProjection>.Empty);
            var jobCount = await _context.Jobs.CountDocumentsAsync(FilterDefinition<AdminJobProjection>.Empty);
            var applicationCount = await _context.Applications.CountDocumentsAsync(FilterDefinition<AdminApplicationProjection>.Empty);

            var activeSeekers = await _context.JobSeekers.CountDocumentsAsync(Builders<AdminJobSeekerProjection>.Filter.Eq(x => x.IsActive, true));
            var activeProviders = await _context.JobProviders.CountDocumentsAsync(Builders<AdminJobProviderProjection>.Filter.Eq(x => x.IsActive, true));

            var activeUsers = activeSeekers + activeProviders;
            var blockedUsers = (jobSeekerCount + jobProviderCount) - activeUsers;

            var hires = await _context.Applications.CountDocumentsAsync(
                Builders<AdminApplicationProjection>.Filter.Eq(x => x.Status, "Hired"));

            return Ok(new
            {
                totalJobSeekers = jobSeekerCount,
                totalJobProviders = jobProviderCount,
                totalJobs = jobCount,
                totalApplications = applicationCount,
                totalHires = hires,
                activeUsers,
                blockedUsers
            });
        }
    }
}
