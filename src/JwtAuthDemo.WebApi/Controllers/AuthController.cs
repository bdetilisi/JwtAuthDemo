using JwtAuthDemo.WebApi.Data.Models.DTOs;
using JwtAuthDemo.WebApi.Data.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthDemo.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private static UserEntity _user = new()
    {
        Email = string.Empty,
        PasswordHashed = string.Empty
    };

    [HttpPost("register")]
    public ActionResult<UserEntity> Register(UserDto request)
    {
        var passwordHasher = new PasswordHasher<UserEntity>();

        _user.Email = request.Email;
        _user.PasswordHashed = passwordHasher.HashPassword(_user, request.Password);

        return Ok(_user);
    }
}
