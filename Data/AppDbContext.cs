using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Camera> Cameras => Set<Camera>();
    public DbSet<CameraOption> CameraOptions => Set<CameraOption>();
    public DbSet<TrackRecognize> RecognizeTracks => Set<TrackRecognize>();
   

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Camera>()
            .HasOne(c => c.Option)
            .WithOne(o => o.Camera)
            .HasForeignKey<CameraOption>(o => o.CameraId)
            .OnDelete(DeleteBehavior.Cascade);;

        modelBuilder.Entity<CameraOption>().ToTable("CameraOptions");

        modelBuilder.Entity<TrackRecognize>(entity =>
        {
            entity.ToTable("RecognizeTracks");

            // связь с камерой
            entity.HasOne(rt => rt.Camera)
                  .WithMany()
                  .HasForeignKey(rt => rt.CameraId)
                  .OnDelete(DeleteBehavior.Restrict);

            // индексы (ОЧЕНЬ важно для LPR)
            entity.HasIndex(rt => rt.PlateNumber);
            entity.HasIndex(rt => rt.CameraId);
            entity.HasIndex(rt => rt.FirstSeen);
            entity.HasIndex(rt => rt.LeftAt);

            // ограничения (по желанию)
            entity.Property(rt => rt.PlateNumber)
                  .HasMaxLength(15);
        });



    }
}