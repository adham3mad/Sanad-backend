using Sanad.DTOs;

namespace Sanad.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        public List<ConversationDto> Conversations { get; set; }
    }

}
