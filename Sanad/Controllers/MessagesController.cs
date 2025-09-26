using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanad.Models.Data;
using Sanad.DTOs;
using SanadAPI.Models.Data;

namespace SanadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly DbEntity context;

        public MessagesController(DbEntity _context)
        {
            context = _context;
        }

       
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto dto)
        {

            var conv = await context.Conversations.FindAsync(dto.ConversationId);
            if (conv == null) return NotFound();

            var message = new Message
            {
                Role = dto.Role,
                Content = dto.Content,
                Conversation_Id = dto.ConversationId,
                CreatedAt = DateTime.Now
            };

            context.Messages.Add(message);
            await context.SaveChangesAsync();

            return new MessageDto
            {
                Id = message.Id,
                Role = message.Role,
                Content = message.Content,
                CreatedAt = message.CreatedAt
            };
        }

        [HttpGet("conversation/{conversationId}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetConversationMessages(int conversationId)
        {

            var conv = await context.Conversations.FindAsync(conversationId);
            if (conv == null) return NotFound();


            var messages = await context.Messages
                .Where(m => m.Conversation_Id == conversationId)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Role = m.Role,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            if (!messages.Any())
                return NotFound($"No messages found for conversation {conversationId}");

            return messages;
        }
    }
}
