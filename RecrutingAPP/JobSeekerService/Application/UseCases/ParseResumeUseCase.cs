using JobSeekerService.Application.DTOs;
using JobSeekerService.Application.Interfaces;

namespace JobSeekerService.Application.UseCases
{
    public class ParseResumeUseCase
    {
        private readonly IResumeParserService _parser;

        public ParseResumeUseCase(IResumeParserService parser)
        {
            _parser = parser;
        }

        public Task<ResumeParseResult> ExecuteAsync(ResumeParseRequest request)
            => _parser.ParseAsync(request);
    }
}
