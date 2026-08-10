using EmotionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Mood> Moods => Set<Mood>();

    public DbSet<MediaItem> MediaItems => Set<MediaItem>();

    public DbSet<ItemType> ItemTypes => Set<ItemType>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
