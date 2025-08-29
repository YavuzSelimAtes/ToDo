using Microsoft.AspNetCore.Mvc;
using ToDoApi.Models;
using ToDoApi.Data;
using ToDoApi.Helpers;

namespace ToDoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ToDoContext _context;

        public AuthController(ToDoContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register(Register dto)
        {
            if (_context.Users.Any(u => u.Username == dto.Username))
                return BadRequest("Username already exists");

            var user = new ToDoUser
            {
                Username = dto.Username,
                PasswordHash = HashPassword.Password(dto.Password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public IActionResult Login(Login dto)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == dto.Username);
            if (user == null) return Unauthorized("User not found");

            var hashed = HashPassword.Password(dto.Password);
            if (user.PasswordHash != hashed) return Unauthorized("Invalid password");

            return Ok(new { Message = "Login successful", UserId = user.Id });
        }
    }
}
