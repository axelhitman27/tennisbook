using Microsoft.AspNetCore.Mvc;
using Tennisbook.API.DTOs;
using Tennisbook.API.Services;

namespace Tennisbook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return result == null
            ? Unauthorized(new { message = "Invalid email or password" })
            : Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return result == null
            ? BadRequest(new { message = "Email already registered" })
            : Ok(result);
    }
}
