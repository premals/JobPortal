namespace JobProviderService.DTO
{
    public class JobProviderProfileDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string CompanySize { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string About { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? TwitterUrl { get; set; }
    }
}
