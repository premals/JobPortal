using JobSeekerService.Application.DTOs;
using JobSeekerService.Application.Interfaces;

namespace JobSeekerService.Application.UseCases
{
    public class ParseResumeFileUseCase
    {
        private readonly IResumeParserService _parser;

        public ParseResumeFileUseCase(IResumeParserService parser)
        {
            _parser = parser;
        }

        public Task<ResumeParseResult> ExecuteAsync(ResumeFileParseRequest request)
            => _parser.ParseFileAsync(request);
    }
}
