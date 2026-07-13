using JwtAuthDemo.WebApi.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthDemo.WebApi.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>()
            .Property(u => u.Role)
             .HasColumnType("int");
    }
}
