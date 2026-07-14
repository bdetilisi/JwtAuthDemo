using JwtAuthDemo.WebApi.Data.User.Entities;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthDemo.WebApi.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; }
}
