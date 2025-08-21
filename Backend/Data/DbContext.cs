namespace Backend.Data;

using Backend.Models;
using Discord;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<DiscordUser> DiscordUsers { get; set; }
    public DbSet<WebsiteUser> WebsiteUsers { get; set; }
    public DbSet<SchoolADUser> SchoolADUsers { get; set; }
    public DbSet<UserActivity> UserActivities { get; set; }
    public DbSet<UserDailyActivity> UserDailyActivities { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<DiscordVerification> DiscordVerifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Konfigurer dine entity mappings her
        modelBuilder.Entity<DiscordUser>().HasIndex(u => u.DiscordId).IsUnique();

        // Email indeks kun for ikke-tomme emails (Discord brugere kan have tomme emails)
        modelBuilder.Entity<WebsiteUser>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("\"Email\" != ''");

        // Username indeks kun for ikke-tomme usernames
        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique()
            .HasFilter("\"UserName\" != ''");

        modelBuilder.Entity<WebsiteUser>()
            .HasIndex(u => u.UserName)
            .IsUnique()
            .HasFilter("\"UserName\" != ''");

        modelBuilder.Entity<User>()
            .HasOne(u => u.DiscordUser)
            .WithOne(du => du.User)
            .HasForeignKey<User>(u => u.DiscordUserId);

        modelBuilder.Entity<User>()
            .HasOne(u => u.WebsiteUser)
            .WithOne(du => du.User)
            .HasForeignKey<User>(u => u.WebsiteUserId);

        modelBuilder.Entity<User>()
            .HasOne(u => u.SchoolADUser)
            .WithOne(du => du.User)
            .HasForeignKey<User>(u => u.SchoolADUserId);

        // RefreshToken konfiguration
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.WebsiteUser)
            .WithMany()
            .HasForeignKey(rt => rt.WebsiteUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();

        // DiscordVerification konfiguration
        modelBuilder.Entity<DiscordVerification>()
            .HasIndex(dv => dv.VerificationCode)
            .IsUnique();

        modelBuilder.Entity<DiscordVerification>()
            .HasIndex(dv => new { dv.DiscordUserId, dv.DiscordId });
    }
}
