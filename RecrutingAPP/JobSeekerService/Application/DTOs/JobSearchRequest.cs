namespace JobSeekerService.Application.DTOs
{
    public class JobSearchRequest
    {
        public string? Keyword { get; set; }
        public string? City { get; set; }
        public string? EmploymentType { get; set; }
        public int? MinExperience { get; set; }
        public decimal? MinSalary { get; set; }
    }
}
