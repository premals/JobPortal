using System.ComponentModel.DataAnnotations;

namespace IdendityService.DTOs
{
    public class RegisterRequest
    {
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string UserType { get; set; }
    }
}
