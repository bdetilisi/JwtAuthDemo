using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthDemo.WebApi.Services.Auth;

public class AuthService(ApplicationDbContext dbContext, TokenGenerator tokenGenerator)
{
    private readonly PasswordHasher<UserEntity> _passwordHasher = new();

    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request)
    {
        bool exists = await dbContext.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
        {
            return Result.Fail<RegisterResponse>("User with this email already exists.");
        }

        var newUser = new UserEntity
        {
            Email = request.Email,
            Role = request.UserRole,
            PasswordHashed = _passwordHasher.HashPassword(null, request.Password)
        };

        var entityEntry = await dbContext.AddAsync(newUser);
        await dbContext.SaveChangesAsync();


        var response = new RegisterResponse
        {
            UserId = entityEntry.Entity.Id,
            Email = entityEntry.Entity.Email,
            UserRole = entityEntry.Entity.Role
        };

        return Result.Ok(response);
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {   
        var userEntity = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email == request.Email);

        if (userEntity == null)
        {
            return Result.Fail<LoginResponse>("User not found.");
        }

        var result = _passwordHasher.VerifyHashedPassword(
            null, // Detilisi: Not used in implementation, can be null.
            userEntity.PasswordHashed, 
            request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Result.Fail<LoginResponse>("Either username or password is incorrect.");
        }

        var tokenResult = tokenGenerator.GenerateAccessToken(userEntity);
        if (tokenResult.IsFailed)
        {
            return Result.Fail<LoginResponse>(tokenResult.Errors);
        }

        var response = new LoginResponse
        {
            UserId = userEntity.Id,
            BearToken = tokenResult.Value
        };

        return Result.Ok(response);
    }
}

