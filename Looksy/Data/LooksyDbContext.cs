using Looksy.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public class LooksyDbContext : DbContext
{
    public LooksyDbContext(DbContextOptions<LooksyDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Photo> Photos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasMany(u => u.Groups)
            .WithMany(g => g.Members)
            .UsingEntity(j => j.ToTable("UserGroups"));

        modelBuilder.Entity<Photo>()
            .HasOne(p => p.UploadedBy)
            .WithMany(u => u.UploadedPhotos)
            .HasForeignKey(p => p.UploadedByUserId);

        modelBuilder.Entity<Photo>()
            .HasOne(p => p.Group)
            .WithMany(g => g.Photos)
            .HasForeignKey(p => p.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
