using JobSeekerService.Domain.Entities;

namespace JobSeekerService.Application.Interfaces
{
    public interface IResumePdfGenerator
    {
        byte[] Generate(ResumeDraft draft);
    }
}
