using AdminService.Domain.Entities;
using AdminService.Infrastructure.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/admin/job-seekers")]
    [Authorize(Policy = "RequireAdmin")]
    public class AdminJobSeekersController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public AdminJobSeekersController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? city)
        {
            var filters = new List<FilterDefinition<AdminJobSeekerProjection>>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLowerInvariant();
                filters.Add(Builders<AdminJobSeekerProjection>.Filter.Or(
                    Builders<AdminJobSeekerProjection>.Filter.Where(x => x.FullName.ToLower().Contains(lower)),
                    Builders<AdminJobSeekerProjection>.Filter.Where(x => x.Email.ToLower().Contains(lower))
                ));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                filters.Add(Builders<AdminJobSeekerProjection>.Filter.Eq(x => x.Location, city));
            }

            var filter = filters.Count > 0
                ? Builders<AdminJobSeekerProjection>.Filter.And(filters)
                : FilterDefinition<AdminJobSeekerProjection>.Empty;

            var results = await _context.JobSeekers.Find(filter).ToListAsync();
            return Ok(results);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetById(string userId)
        {
            var result = await _context.JobSeekers.Find(x => x.UserId == userId).FirstOrDefaultAsync();
            return result == null ? NotFound() : Ok(result);
        }
    }
}
