using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly AuthService _authService;

        public AuthController(DataContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto dto)
        {
            if (_context.Users.Any(u => u.Username == dto.Username))
                return BadRequest("User existiert bereits.");

            var user = new Users { Username = dto.Username };
            user.PasswordHash = _authService.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User erfolgreich registriert.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == dto.Username);
            if (user == null || !_authService.VerifyPassword(user, user.PasswordHash, dto.Password))
                return Unauthorized("Ungültige Anmeldedaten.");

            // 2. JWT erstellen
            var token = _authService.CreateToken(user);

            return Ok(new { Token = token });
        }


    }
}
