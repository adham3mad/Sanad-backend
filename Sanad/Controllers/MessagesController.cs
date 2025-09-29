using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanad.Models.Data;
using Sanad.DTOs;
using SanadAPI.Models.Data;
using Sanad.Models;

namespace SanadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]

    public class MessagesController : ControllerBase
    {
        private readonly DbEntity context;

        public MessagesController(DbEntity _context)
        {
            context = _context;
        }

        

        

        [HttpPost]
        public async Task<ActionResult> CreateMessage(CreateMessageDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Invalid request body");

                if (dto.ConversationId == 0)
                    return BadRequest("ConversationId is required");

                var conv = await context.Conversations.FindAsync(dto.ConversationId);
                if (conv == null)
                    return NotFound($"Conversation with ID {dto.ConversationId} not found");

                if (string.IsNullOrWhiteSpace(dto.Content))
                    return BadRequest("Message content is required");
                var userMessage = new Message
                {
                    Role = dto.Role,
                    Content = dto.Content.Trim(),
                    Conversation_Id = dto.ConversationId,
                    CreatedAt = DateTime.UtcNow
                };

                context.Messages.Add(userMessage);
                await context.SaveChangesAsync();
                var aiResponse = await CallAiApiAsync(dto.Content);

                if (string.IsNullOrWhiteSpace(aiResponse))
                    aiResponse = "No response from AI.";
                var aiMessage = new Message
                {
                    Role = "AI",
                    Content = aiResponse,
                    Conversation_Id = dto.ConversationId,
                    CreatedAt = DateTime.UtcNow
                };

                context.Messages.Add(aiMessage);
                await context.SaveChangesAsync();
                return Ok(new
                {
                    aiMessage = new MessageDto
                    {
                        Id = aiMessage.Id,
                        Role = aiMessage.Role,
                        Content = aiMessage.Content,
                        CreatedAt = aiMessage.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server Error: {ex.InnerException?.Message ?? ex.Message}");
            }
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

        private async Task<string> CallAiApiAsync(string message)
        {
            using var client = new HttpClient();
            var content = new StringContent($"\"{message}\"", System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://sanad-ai.up.railway.app/api/v1/data/query/1", content);
                var responseString = await response.Content.ReadAsStringAsync();

                Console.WriteLine("API Response: " + responseString);

                if (!response.IsSuccessStatusCode)
                    return $"API Error: {response.StatusCode}";

                var json = System.Text.Json.JsonSerializer.Deserialize<AiApiResponse>(responseString);

                if (!string.IsNullOrWhiteSpace(json?.response))
                    return json.response;

                if (!string.IsNullOrWhiteSpace(json?.answer))
                    return json.answer;

                return "No valid AI response found.";
            }
            catch (Exception ex)
            {
                return $"Error calling AI API: {ex.Message}";
            }
        }



    }
}
