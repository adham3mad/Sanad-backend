namespace Sanad.DTOs
{
    public class ResetPasswordDto
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        [StrongPassword]
        public string NewPassword { get; set; }
    }

}
