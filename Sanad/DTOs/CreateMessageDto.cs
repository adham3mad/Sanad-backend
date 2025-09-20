using System.ComponentModel.DataAnnotations;

namespace Sanad.DTOs
{
    public class CreateMessageDto
    {
        public string Role { get; set; }
        public string Content { get; set; }
        [Required(ErrorMessage = "Conversation ID is required")]
        public int ConversationId { get; set; }
    }
}
