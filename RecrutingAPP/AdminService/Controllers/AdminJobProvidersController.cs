using AdminService.Domain.Entities;
using AdminService.Infrastructure.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/admin/job-providers")]
    [Authorize(Policy = "RequireAdmin")]
    public class AdminJobProvidersController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public AdminJobProvidersController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? city)
        {
            var filters = new List<FilterDefinition<AdminJobProviderProjection>>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLowerInvariant();
                filters.Add(Builders<AdminJobProviderProjection>.Filter.Or(
                    Builders<AdminJobProviderProjection>.Filter.Where(x => x.CompanyName.ToLower().Contains(lower)),
                    Builders<AdminJobProviderProjection>.Filter.Where(x => x.ContactName.ToLower().Contains(lower)),
                    Builders<AdminJobProviderProjection>.Filter.Where(x => x.ContactEmail.ToLower().Contains(lower))
                ));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                filters.Add(Builders<AdminJobProviderProjection>.Filter.Eq(x => x.Location, city));
            }

            var filter = filters.Count > 0
                ? Builders<AdminJobProviderProjection>.Filter.And(filters)
                : FilterDefinition<AdminJobProviderProjection>.Empty;

            var results = await _context.JobProviders.Find(filter).ToListAsync();
            return Ok(results);
        }

        [HttpGet("{providerId}")]
        public async Task<IActionResult> GetById(string providerId)
        {
            var result = await _context.JobProviders.Find(x => x.JobProviderId == providerId).FirstOrDefaultAsync();
            return result == null ? NotFound() : Ok(result);
        }
    }
}
