using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Models.Storage;

namespace WebApi.Infrastructure.Components;

public class DatabaseContext : DbContext
{
    private readonly string _connectionString;
    
    public DatabaseContext()
    {
        _connectionString = "Server=localhost;Port=5438;User Id=postgres;Password=12345;Database=egeDb";
    }

    public DatabaseContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<Variant>().ToTable("variants");
        modelBuilder.Entity<BlockUser>().ToTable("block_users");
        modelBuilder.Entity<Exercise>().ToTable("exercises");
        modelBuilder.Entity<Group>().ToTable("groups");
        modelBuilder.Entity<GroupStudent>().ToTable("group_students");
        modelBuilder.Entity<StudentExercise>().ToTable("student_exercises");
        modelBuilder.Entity<VariantAssignment>().ToTable("variant_assignments");
        modelBuilder.Entity<VariantExercise>().ToTable("variant_exercises");
        modelBuilder.Entity<Role>().ToTable("roles").HasData(
            [new Role { Id = 1, RoleName = "Student"},
             new Role { Id = 2, RoleName = "Teacher"},]);
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Variant> Variants { get; set; }
    public DbSet<Group> Groups { get; set; } 
    public DbSet<BlockUser> BlockUsers { get; set; } 
    public DbSet<Exercise> Exercises { get; set; } 
    public DbSet<GroupStudent> GroupStudents { get; set; } 
    public DbSet<Role> Roles { get; set; } 
    public DbSet<StudentExercise> StudentExercises { get; set; } 
    public DbSet<VariantAssignment> VariantAssignments { get; set; } 
    public DbSet<VariantExercise> VariantExercises { get; set; } 
}