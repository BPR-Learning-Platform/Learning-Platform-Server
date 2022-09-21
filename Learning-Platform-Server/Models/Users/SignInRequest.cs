using System.ComponentModel.DataAnnotations;

namespace Learning_Platform_Server.Models.Users
{
    public class SignInRequest
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
