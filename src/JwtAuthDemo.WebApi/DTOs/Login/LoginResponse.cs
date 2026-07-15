namespace JwtAuthDemo.WebApi.DTOs.Login;

public class LoginResponse
{
    public int UserId { get; set; }
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}
