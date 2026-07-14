namespace JwtAuthDemo.WebApi.DTOs.Register;

public class RegisterRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string UserRole { get; set; }

}
