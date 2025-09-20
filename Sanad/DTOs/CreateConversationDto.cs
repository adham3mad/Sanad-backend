using System.ComponentModel.DataAnnotations;

namespace Sanad.DTOs
{
    public class CreateConversationDto
    {
        public string Title { get; set; }
        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }
    }
}
