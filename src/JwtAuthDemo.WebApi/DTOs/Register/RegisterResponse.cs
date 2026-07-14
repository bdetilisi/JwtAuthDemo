namespace JwtAuthDemo.WebApi.DTOs.Register;

public class RegisterResponse
{
    public int UserId { get; set; }
    public required string Email { get; set; }
    public required string UserRole { get; set; }
}
