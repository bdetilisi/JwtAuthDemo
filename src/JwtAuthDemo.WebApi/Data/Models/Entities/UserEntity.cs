namespace JwtAuthDemo.WebApi.Data.Models.Entities;

public class UserEntity
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHashed { get; set; }
    public required string Role { get; set; }
}

public class UserRoles
{
    public const string User = nameof(UserRoles.User);
    public const string Admin = nameof(UserRoles.Admin);
}
