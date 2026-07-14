namespace JwtAuthDemo.WebApi.DTOs.Login;

public class LoginResponse
{
    public int UserId { get; set; }
    public required string BearToken { get; set; }
}
