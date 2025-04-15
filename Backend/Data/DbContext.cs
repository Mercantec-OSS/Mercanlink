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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Konfigurer dine entity mappings her
        modelBuilder.Entity<User>().HasIndex(u => u.DiscordId).IsUnique();

    }
}
