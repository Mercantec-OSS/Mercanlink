namespace Backend.Data;

using Backend.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<UserActivity> UserActivities { get; set; }
    public DbSet<UserDailyActivity> UserDailyActivities { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<DiscordVerification> DiscordVerifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Konfigurer dine entity mappings her
        modelBuilder.Entity<User>().HasIndex(u => u.DiscordId).IsUnique();
        
        // Email indeks kun for ikke-tomme emails (Discord brugere kan have tomme emails)
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("\"Email\" != ''");
            
        // Username indeks kun for ikke-tomme usernames
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique()
            .HasFilter("\"Username\" != ''");

        // RefreshToken konfiguration
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();

        // DiscordVerification konfiguration
        modelBuilder.Entity<DiscordVerification>()
            .HasIndex(dv => dv.VerificationCode)
            .IsUnique();

        modelBuilder.Entity<DiscordVerification>()
            .HasIndex(dv => new { dv.UserId, dv.DiscordId });
    }
}
