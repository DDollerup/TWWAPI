using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TWWAPI.Models;

namespace TWWAPI.Controllers
{
    public class LoginUser
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public int? RoleId { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly TWWContext _context;
        private readonly IConfiguration _configuration;

        public LoginController(TWWContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUser loginUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginUser.Email);

            if (user == null)
            {
                ModelState.AddModelError("Email", "Invalid email or password");
                return BadRequest(ModelState);
            }

            // Decode password hash and salt
            byte[] passwordHash = Convert.FromBase64String(user.PasswordHash);
            byte[] passwordSalt = Convert.FromBase64String(user.PasswordSalt);

            if (!VerifyPasswordHash(loginUser.Password, passwordHash, passwordSalt))
            {
                ModelState.AddModelError("Email", "Invalid email or password");
                return BadRequest(ModelState);
            }

            // Login successful
            // Create JWT token
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, _configuration["Jwt:Audience"]),
                new Claim(JwtRegisteredClaimNames.Iss, _configuration["Jwt:Issuer"]),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Set authentication cookie
            Response.Cookies.Append(
                "jwt",
                tokenHandler.WriteToken(token),
                new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    Secure = true,
                    Expires = DateTime.UtcNow.AddHours(1),
                    Path = "/"
                }
            );

            // Return user data and token in response
            var response = new
            {
                user.Id,
                user.Email,
                Token = tokenHandler.WriteToken(token)
            };

            return Ok(response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] LoginUser newUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == newUser.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email already in use");
                return BadRequest(ModelState);
            }

            // Generate password hash and salt
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(newUser.Password, out passwordHash, out passwordSalt);

            // Create new user entity
            var user = new User
            {
                Email = newUser.Email,
                PasswordHash = Convert.ToBase64String(passwordHash),
                PasswordSalt = Convert.ToBase64String(passwordSalt)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();

            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt);

            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != passwordHash[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
