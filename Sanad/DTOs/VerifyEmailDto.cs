namespace Sanad.DTOs
{
    public class VerifyEmailDto
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}
