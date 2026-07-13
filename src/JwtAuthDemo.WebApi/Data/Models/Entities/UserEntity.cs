namespace JwtAuthDemo.WebApi.Data.Models.Entities;

public class UserEntity
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHashed { get; set; }
    public UserRole Role { get; set; }
}

public enum UserRole
{
    User = 0,
    Admin = 1
}
