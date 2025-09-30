
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sanad.DTOs;
using Sanad.Models.Data;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Sanad.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DbEntity context;
        private readonly IConfiguration config;
        private readonly EmailSettings emailSettings;

        private static Dictionary<Guid, (string Token, DateTime Expiry)> verificationTokens = new();

        public AuthController(DbEntity _context, IConfiguration _config, IOptions<EmailSettings> _emailSettings)
        {
            context = _context;
            config = _config;
            emailSettings = _emailSettings.Value;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await context.Users.AnyAsync(u => u.Email == model.Email))
                return BadRequest("Email already exists");

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role,
                IsEmailConfirmed = false
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var expiry = DateTime.UtcNow.AddHours(24);
            verificationTokens[user.Id] = (token, expiry);
            var encodedToken = System.Web.HttpUtility.UrlEncode(token);

            var verificationLink = $"https://sanad-legal-ai.netlify.app/#/verify_email?userId={user.Id}&token={encodedToken}";

            try
            {
                await SendEmailAsync(
                    user.Email,
                    user.Name,
                    "Verify your email",
                    $@"
                    <h2>Welcome {user.Name}</h2>
                    <p>Please verify your email by clicking the link below:</p>
                    <p><a href='{verificationLink}' target='_blank'>Verify Email</a></p>
                    <br/>
                    <p>This link will expire in 24 hours.</p>"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }

            return Ok(new { message = "User registered successfully. Please check your email to verify your account." });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto request)
        {
            var userId = request.userId;
            var token = request.token;

            if (!verificationTokens.ContainsKey(userId))
                return BadRequest("Invalid or expired token");

            var (savedToken, expiry) = verificationTokens[userId];
            if (savedToken != token || expiry < DateTime.UtcNow)
                return BadRequest("Invalid or expired token");

            var user = await context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            user.IsEmailConfirmed = true;
            await context.SaveChangesAsync();

            verificationTokens.Remove(userId);

            return Ok(new { message = "Email verified successfully!" });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                var user = await context.Users
                    .Include(u => u.Conversations)
                    .ThenInclude(c => c.Messages)
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user == null)
                    return NotFound(new { message = "Email not found" });
                if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                    return Unauthorized(new { message = "Invalid password" });
                if (!user.IsEmailConfirmed)
                    return Unauthorized(new { message = "Please verify your email before logging in." });
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Image = user.Image,
                    Email = user.Email,
                    Role = user.Role,
                    Conversations = user.Conversations.Select(c => new ConversationDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        CreatedAt = c.CreatedAt,
                        Messages = c.Messages.Select(m => new MessageDto
                        {
                            Id = m.Id,
                            Role = m.Role,
                            Content = m.Content,
                            CreatedAt = m.CreatedAt
                        }).ToList()
                    }).ToList()
                };
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Login failed: {ex.Message}" });
            }
        }



        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordDto dto)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return NotFound("Email not found");

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var expiry = DateTime.UtcNow.AddMinutes(15);
            verificationTokens[user.Id] = (token, expiry);

            var resetLink = $"https://sanad-legal-ai.netlify.app//#/reset_password?userId={user.Id}&token={token}";

            try
            {
                await SendEmailAsync(
                    user.Email,
                    user.Name,
                    "Password Reset Request",
                    $@"
                    <h2>Password Reset Request</h2>
                    <p>We received a request to reset your password.</p>
                    <p>Click the link below to reset your password:</p>
                    <p><a href='{resetLink}' target='_blank'>Reset Password</a></p>
                    <br/>
                    <p><b>Note:</b> This link will expire in 15 minutes.</p>
                    <p>If you didn't request this, please ignore this email.</p>"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }

            return Ok("Password reset link has been sent to your email (valid 15 minutes)");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!verificationTokens.TryGetValue(dto.UserId, out var tokenData))
                return BadRequest("Invalid or expired token");

            var (storedToken, expiry) = tokenData;
            if (storedToken != dto.Token || DateTime.UtcNow > expiry)
                return BadRequest("Invalid or expired token");

            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (user == null) return NotFound("User not found");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            verificationTokens.Remove(dto.UserId);

            await context.SaveChangesAsync();
            return Ok("Password has been reset successfully");
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var jwtKey = config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing from configuration");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlContent)
        {
            var client = new SendGridClient(emailSettings.ApiKey);

            var from = new EmailAddress(emailSettings.SenderEmail, emailSettings.SenderName);
            var to = new EmailAddress(toEmail, toName);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: "", htmlContent: htmlContent);

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid failed: {response.StatusCode}, {body}");
            }
        }
    }
}
