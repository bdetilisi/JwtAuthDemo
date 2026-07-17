using JwtAuthDemo.WebApi.DTOs.Token;
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

        var passwordVerifyResult = _passwordHasher.VerifyHashedPassword(
            null, // Detilisi: Not used in implementation, can be null.
            userEntity.PasswordHashed, 
            request.Password);

        if (passwordVerifyResult == PasswordVerificationResult.Failed)
        {
            return Result.Fail<LoginResponse>("Either username or password is incorrect.");
        }

        var tokenResult = tokenGenerator.GenerateAccessToken(userEntity);
        if (tokenResult.IsFailed)
        {
            return Result.Fail<LoginResponse>(tokenResult.Errors);
        }

        var refreshTokenResult = tokenGenerator.GenerateRefreshToken();
        if (refreshTokenResult.IsFailed)
        {
            return Result.Fail<LoginResponse>(refreshTokenResult.Errors);
        }

        var response = new LoginResponse
        {
            UserId = userEntity.Id,
            AccessToken = tokenResult.Value,
            RefreshToken = refreshTokenResult.Value
        };

        // Save refresh token to database
        _ = await dbContext.Users.ExecuteUpdateAsync(u => u
            .SetProperty(user => user.RefreshToken, refreshTokenResult.Value)
            .SetProperty(user => user.RefreshTokenExpiryTime, DateTime.Now.AddDays(7))
        );

        return Result.Ok(response);
    }


    public async Task<Result<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var userEntity = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == request.UserId);

        if (userEntity == null)
        {
            return Result.Fail<RefreshTokenResponse>("User not found.");
        }

        bool isRefreshTokenValid = 
            !string.IsNullOrEmpty(userEntity.RefreshToken) && 
            !string.IsNullOrEmpty(request.RefreshToken) && 
            userEntity.RefreshToken == request.RefreshToken && 
            userEntity.RefreshTokenExpiryTime > DateTime.Now;

        if (!isRefreshTokenValid)
        {
            return Result.Fail<RefreshTokenResponse>("Invalid or expired refresh token.");
        }

        var tokenResult = tokenGenerator.GenerateAccessToken(userEntity);
        if (tokenResult.IsFailed)
        {
            return Result.Fail<RefreshTokenResponse>(tokenResult.Errors);
        }

        var refreshTokenResult = tokenGenerator.GenerateRefreshToken();
        if (refreshTokenResult.IsFailed)
        {
            return Result.Fail<RefreshTokenResponse>(refreshTokenResult.Errors);
        }

        // Save new refresh token to database
        _ = await dbContext.Users.ExecuteUpdateAsync(u => u
            .SetProperty(user => user.RefreshToken, refreshTokenResult.Value)
            .SetProperty(user => user.RefreshTokenExpiryTime, DateTime.Now.AddDays(7))
        );

        var response = new RefreshTokenResponse
        {
            AccessToken = tokenResult.Value,
            RefreshToken = refreshTokenResult.Value
        };
        return Result.Ok(response);
    }
}

