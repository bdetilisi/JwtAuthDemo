using JwtAuthDemo.WebApi.Data.Models.DTOs;
using JwtAuthDemo.WebApi.Data.Models.Entities;
using JwtAuthDemo.WebApi.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthDemo.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(AuthService authService) : ControllerBase
{
    
    [HttpPost("register")]
    public async Task<ActionResult<UserEntity>> Register(UserDto request)
    {
        var result = await authService.RegisterAsync(request);
        if (result.IsFailed)
        {
            string errorMessage = result.Errors.FirstOrDefault()?.Message 
                ?? "Registration failed.";
            return BadRequest(errorMessage);
        }

        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(UserDto request)
    {
        var result = await authService.LoginAsync(request);
        if (result.IsFailed)
        {
            string errorMessage = result.Errors.FirstOrDefault()?.Message
                ?? "Login failed.";
            return Unauthorized(errorMessage);
        }

        return Ok(result.Value);
    }
}
