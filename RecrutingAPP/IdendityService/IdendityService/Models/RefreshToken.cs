namespace IdendityService.Models
{
    public class RefreshToken
    {
        public string Token { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public bool Revoked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedByIp { get; set; } = default!;
    }
}
