using System.ComponentModel.DataAnnotations;

namespace Sanad.DTOs
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters")]
        public string Name { get; set; }
        public string Image { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StrongPassword]
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "User";

    }
}
