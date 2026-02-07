using AdminService.Domain.Entities;
using AdminService.Infrastructure.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/admin/jobs")]
    [Authorize(Policy = "RequireAdmin")]
    public class AdminJobsController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public AdminJobsController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var filter = string.IsNullOrWhiteSpace(status)
                ? FilterDefinition<AdminJobProjection>.Empty
                : Builders<AdminJobProjection>.Filter.Eq(x => x.Status, status);

            var results = await _context.Jobs.Find(filter).ToListAsync();
            return Ok(results);
        }

        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetById(string jobId)
        {
            var result = await _context.Jobs.Find(x => x.JobId == jobId).FirstOrDefaultAsync();
            return result == null ? NotFound() : Ok(result);
        }
    }
}
