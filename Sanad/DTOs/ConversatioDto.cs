using System.ComponentModel.DataAnnotations;

namespace Sanad.DTOs
{
    public class ConversationDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MessageDto> Messages { get; set; }
    }
}
