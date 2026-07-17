using Microsoft.AspNetCore.Mvc;

namespace JwtAuthDemo.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost(nameof(Register))]
    public async Task<ActionResult<UserEntity>> Register(RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);

        if (result.IsFailed)
        {
            string errorMessage = string.Join(
                $";{Environment.NewLine}", 
                result.Errors.Select(x => x.Message)
            );

            return BadRequest(errorMessage);
        }

        return Ok(result.Value);
    }

    [HttpPost(nameof(Login))]
    public async Task<ActionResult<string>> Login(LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        if (result.IsFailed)
        {
            string errorMessage = string.Join(
                $";{Environment.NewLine}",
                result.Errors.Select(x => x.Message));

            return Unauthorized(errorMessage);
        }

        return Ok(result.Value);
    }

    [HttpPost(nameof(RefreshToken))]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request)
    {
        var result = await authService.RefreshTokenAsync(request);
        if (result.IsFailed)
        {
            string errorMessage = string.Join(
                $";{Environment.NewLine}",
                result.Errors.Select(x => x.Message));
            return Unauthorized(errorMessage);
        }
        return Ok(result.Value);
    }
}
