namespace IdendityService.DTOs
{
    public class AdminUserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool ForcePasswordReset { get; set; }
    }

    public class UpdateUserStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class UpdateUserRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }
}
