using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JwtAuthDemo.WebApi.Services.Auth;

public class TokenGenerator(IConfiguration configuration)
{
    public Result<string> GenerateAccessToken(UserEntity user)
    {
        //options
        JwtOptions jwtOptionsDto = configuration
            .GetRequiredSection("JwtOptions")
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException("JwtOptions section is missing in configuration.");

        //create claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
        };

        // create credentials
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtOptionsDto.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        // create token descriptor
        var tokenDescriptor = new JwtSecurityToken(
            issuer: jwtOptionsDto.Issuer,
            audience: jwtOptionsDto.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(jwtOptionsDto.ExpiryMinutes),
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();

        var accessToken = tokenHandler.WriteToken(tokenDescriptor);
        return Result.Ok(accessToken);
    }

    public Result<string> GenerateAccessToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshToken = Convert.ToBase64String(randomBytes);
        return Result.Ok(refreshToken);
    }
}
