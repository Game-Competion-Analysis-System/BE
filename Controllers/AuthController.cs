using BIL.Service;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, Swd392GameAiContext context) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly Swd392GameAiContext _context = context;

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _authService.Authenticate(request.Email, request.Password);

        if (user == null)
            return Unauthorized(new { message = "Invalid email or password" });

        var token = _authService.GenerateJwtToken(user);

        return Ok(new
        {
            Token = token,
            User = new
            {
                user.Userid,
                user.Username,
                user.Email,
                user.Role
            }
        });
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (_context.Users.Any(u => u.Email == request.Email))
            return BadRequest(new { message = "Email already exists" });

        if (_context.Users.Any(u => u.Username == request.Username))
            return BadRequest(new { message = "Username already exists" });

        var newUser = new User
        {
            Username = request.Username,
            Email = request.Email,
            Passwordhash = request.Password, // In a real app, hash this!
            Role = request.Role ?? "user"
        };

        _context.Users.Add(newUser);
        _context.SaveChanges();

        return Ok(new { message = "User registered successfully" });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Role { get; set; }
}
