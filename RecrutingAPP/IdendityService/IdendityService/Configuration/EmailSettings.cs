namespace IdendityService.Configuration
{
    public class EmailSettings
    {
        public string From { get; set; } = string.Empty;
        public string? FromName { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string UserName { get; set; } = string.Empty;
        public string <secret> { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }
}
