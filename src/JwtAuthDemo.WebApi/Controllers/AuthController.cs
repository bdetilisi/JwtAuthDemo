using JwtAuthDemo.WebApi.Data.Models.DTOs;
using JwtAuthDemo.WebApi.Data.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuthDemo.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IConfiguration configuration) : ControllerBase
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

        string token = CreateToken(_userEntity);
        return Ok(token);
    }

    [NonAction]
    private string CreateToken(UserEntity user)
    {
        //options
        var issuer = configuration.GetValue<string>("JwtOptions:Issuer");
        var audience = configuration.GetValue<string>("JwtOptions:Audience");
        var secretKey = configuration.GetValue<string>("JwtOptions:SecretKey");

        //create claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email)
        };

        // create credentials
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        // create token descriptor
        var tokenDescriptor1 = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(1),
            SigningCredentials = credentials
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenDescriptor);
    }
}
