using JobSeekerService.Application.DTOs;
using JobSeekerService.Application.Interfaces;
using JobSeekerService.Domain.Entities;

namespace JobSeekerService.Application.UseCases
{
    public class BrowseJobsUseCase
    {
        private readonly IJobReadRepository _repo;

        public BrowseJobsUseCase(IJobReadRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<JobSnapshot>> GetAllAsync(int page, int pageSize)
            => await _repo.GetAllAsync(page, pageSize);

        public async Task<List<JobSnapshot>> SearchAsync(JobSearchRequest req)
            => await _repo.SearchAsync(req);
    }
}
