namespace Sanad.DTOs
{
    public class VerifyEmailDto
    {
        public Guid userId { get; set; }
        public string token { get; set; }
    }
}
