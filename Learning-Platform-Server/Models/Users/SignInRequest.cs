using System.ComponentModel.DataAnnotations;

namespace Learning_Platform_Server.Models.Users
{
    public class SignInRequest
    {
        private string? _email;

        [Required]
        [EmailAddress]
        public string? Email
        {
            get { return _email; }
            set { _email = ("" + value).ToLower(); }
        }

        [Required]
        public string? Password { get; set; }
    }
}
