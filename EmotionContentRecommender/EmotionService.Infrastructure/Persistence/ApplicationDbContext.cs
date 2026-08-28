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

    public DbSet<Genre> Genres => Set<Genre>();

    public DbSet<MediaItemGenre> MediaItemGenres => Set<MediaItemGenre>();

    public DbSet<MusicDetail> MusicDetails => Set<MusicDetail>();

    public DbSet<MovieDetail> MovieDetails => Set<MovieDetail>();

    public DbSet<GameDetail> GameDetails => Set<GameDetail>();

    public DbSet<BookDetail> BookDetails => Set<BookDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
