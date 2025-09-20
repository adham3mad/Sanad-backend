using System.ComponentModel.DataAnnotations;

namespace Sanad.DTOs
{
    public class RegisterDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters.")]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StrongPassword]
        public string Password { get; set; }

        public string Role { get; set; } = "User";
    }
}
