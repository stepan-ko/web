using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Camera> Cameras => Set<Camera>();
    public DbSet<CameraOption> CameraOptions => Set<CameraOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Camera>()
            .HasOne(c => c.Option)
            .WithOne(o => o.Camera)
            .HasForeignKey<CameraOption>(o => o.CameraId);
    }
}