namespace Sanad.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string Content { get; set; }
        public string? SourceDocs { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
