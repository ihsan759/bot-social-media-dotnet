
using BotSocialMedia.Models;
using Microsoft.EntityFrameworkCore;

namespace BotSocialMedia.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Accounts> Accounts => Set<Accounts>();
    public DbSet<Bots> Bots => Set<Bots>();
    public DbSet<Customs> Customs => Set<Customs>();
    public DbSet<Question> Questions => Set<Question>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Accounts>()
            .HasMany(a => a.Bots)
            .WithOne(b => b.Account)
            .HasForeignKey(b => b.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Bots>()
            .HasMany(b => b.Customs)
            .WithOne(c => c.Bots)
            .HasForeignKey(c => c.BotsId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Customs>()
            .HasMany(c => c.Questions)
            .WithOne(q => q.Customs)
            .HasForeignKey(q => q.CustomsId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
