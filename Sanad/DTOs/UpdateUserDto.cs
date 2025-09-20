using System.ComponentModel.DataAnnotations;

namespace Sanad.DTOs
{
    public class UpdateUserDto
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters")]
        public string? Name { get; set; }
        public string? Image { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [StrongPassword(ErrorMessage = "Password must have at least 1 uppercase, 1 lowercase, 1 number, and 1 special character.")]
        public string? PasswordHash { get; set; }
        public string? Role { get; set; }

    }
}
