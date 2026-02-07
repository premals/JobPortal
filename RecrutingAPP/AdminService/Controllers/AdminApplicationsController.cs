using AdminService.Domain.Entities;
using AdminService.Infrastructure.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/admin/applications")]
    [Authorize(Policy = "RequireAdmin")]
    public class AdminApplicationsController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public AdminApplicationsController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var filter = string.IsNullOrWhiteSpace(status)
                ? FilterDefinition<AdminApplicationProjection>.Empty
                : Builders<AdminApplicationProjection>.Filter.Eq(x => x.Status, status);

            var results = await _context.Applications.Find(filter).ToListAsync();
            return Ok(results);
        }

        [HttpGet("{applicationId}")]
        public async Task<IActionResult> GetById(string applicationId)
        {
            var result = await _context.Applications.Find(x => x.ApplicationId == applicationId).FirstOrDefaultAsync();
            return result == null ? NotFound() : Ok(result);
        }
    }
}
