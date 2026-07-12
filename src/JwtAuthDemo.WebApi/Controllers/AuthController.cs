using JwtAuthDemo.WebApi.Data.Models.DTOs;
using JwtAuthDemo.WebApi.Data.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthDemo.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private static readonly UserEntity _userEntity = new()
    {
        Email = string.Empty,
        PasswordHashed = string.Empty
    };

    private static readonly PasswordHasher<UserEntity> _passwordHasher = new();

    [HttpPost("register")]
    public ActionResult<UserEntity> Register(UserDto request)
    {
        _userEntity.Email = request.Email;
        _userEntity.PasswordHashed = _passwordHasher.HashPassword(_userEntity, request.Password);

        return Ok(_userEntity);
    }

    [HttpPost("login")]
    public ActionResult<string> Login(UserDto request)
    {
        if(_userEntity.Email != request.Email)
        {
            return Unauthorized("User not found.");
        }

        var result = _passwordHasher.VerifyHashedPassword(_userEntity, _userEntity.PasswordHashed, request.Password);
        if(result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Either username or password is incorrect");
        }

        string token = "success";
        return Ok(token);
    }
}
