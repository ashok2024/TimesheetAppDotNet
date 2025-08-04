
using Microsoft.AspNetCore.Mvc;
using TimesheetApp.Application.DTOs;
using TimesheetApp.Application.Services;
using TimesheetApp.Domain.Entities;

namespace TimesheetApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly JwtService _jwtService;

        public AuthController(IAuthService authService, JwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (await _authService.UsernameExistsAsync(request.Username))
                return BadRequest("Username already exists.");

            var user = await _authService.RegisterAsync(request);

            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _authService.GetUserByUsernameAsync(request.Username);
            //if (user == null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            //    return Unauthorized("Invalid credentials.");

            var token = _jwtService.GenerateToken(user.Id, user.UserName, user.Role);
            return Ok(new { token });
        }
    }
}
