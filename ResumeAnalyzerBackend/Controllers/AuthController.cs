using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ResumeAnalyzerBackend;
using ResumeAnalyzerBackend.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ResumeAnalyzerBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly MongoDBService _mongoService;
        private readonly IConfiguration _config;

        public AuthController(MongoDBService mongoService, IConfiguration config)
        {
            _mongoService = mongoService;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginUser)
        {
            var user = await _mongoService.GetUserByEmail(loginUser.Email);
            if (user == null || user.PasswordHash != loginUser.PasswordHash)
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = GenerateJwtToken(user.Email);
            return Ok(new { token, user.Email });
        }
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Optionally log or blacklist token if you're managing sessions (advanced)
            return Ok("User logged out successfully.");
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            var existingUser = await _mongoService.GetUserByEmail(user.Email);
            if (existingUser != null)
                return BadRequest("User already exists.");

            await _mongoService.AddUser(user);
            return Ok("User registered successfully.");
        }

        private string GenerateJwtToken(string email)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
