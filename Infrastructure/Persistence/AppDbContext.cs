using Microsoft.EntityFrameworkCore;
using Practica_Ing_de_software_I_.Domain.Entities;

namespace Practica_Ing_de_software_I_.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de la entidad User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.PasswordHash)
                .IsRequired();

            entity.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(50);
        });

        // Configuración de la entidad Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Description)
                .HasMaxLength(1000);

            entity.Property(p => p.Status)
                .IsRequired()
                .HasMaxLength(50);

            // Relación 1:N entre User (Student) y Project
            entity.HasOne(p => p.Student)
                .WithMany(u => u.Projects)
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

