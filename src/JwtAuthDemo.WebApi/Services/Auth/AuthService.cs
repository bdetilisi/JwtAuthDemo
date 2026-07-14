using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuthDemo.WebApi.Services.Auth;

public class AuthService(ApplicationDbContext dbContext, IConfiguration configuration)
{
    private readonly PasswordHasher<UserEntity> _passwordHasher = new();

    public async Task<Result<UserEntity>> RegisterAsync(RegisterRequest request)
    {
        bool exists = await dbContext.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
        {
            return Result.Fail<UserEntity>("User with this email already exists.");
        }

        var newUser = new UserEntity
        {
            Email = request.Email,
            Role = request.UserRole,
            PasswordHashed = _passwordHasher.HashPassword(null, request.Password)
        };

        var entityEntry = await dbContext.AddAsync(newUser);
        await dbContext.SaveChangesAsync();

        return Result.Ok(entityEntry.Entity);
    }

    public async Task<Result<string>> LoginAsync(LoginRequest request)
    {   
        var userEntity = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email == request.Email);

        if (userEntity == null)
        {
            return Result.Fail<string>("User not found.");
        }

        var result = _passwordHasher.VerifyHashedPassword(
            null, // Detilisi: Not used in implementation, can be null.
            userEntity.PasswordHashed, 
            request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Result.Fail<string>("Either username or password is incorrect.");
        }

        string token = this.CreateToken(userEntity);
        return Result.Ok(token);
    }

    //Helpers
    private string CreateToken(UserEntity user)
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
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenDescriptor);
    }

}
