using Microsoft.EntityFrameworkCore;
public class PlateEventWorker : BackgroundService
{
    private readonly IPlateEventService _service;
    private readonly IServiceScopeFactory _scopeFactory;

    public PlateEventWorker(
        IPlateEventService service,
        IServiceScopeFactory scopeFactory)
    {
        _service = service;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var evt in _service.ReadAllAsync(stoppingToken))
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await Handle(evt, db);
        }
    }

    private async Task Handle(PlateEvent evt, AppDbContext db)
    {
        var track = await db.Set<TrackRecognize>()
            .FirstOrDefaultAsync(x =>
                x.CameraId == evt.CameraId &&
                x.PlateNumber == evt.PlateNumber &&
                x.LeftAt == null);

        switch (evt.Type)
        {
            case PlateEventType.Detect:
                if (track == null)
                {
                    db.Add(new TrackRecognize
                    {
                        CameraId = evt.CameraId,
                        PlateNumber = evt.PlateNumber,
                        FirstSeen = evt.Timestamp,
                        LastSeen = evt.Timestamp
                    });
                }
                break;

            case PlateEventType.Active:
                if (track != null)
                    track.LastSeen = evt.Timestamp;
                break;

            case PlateEventType.Lost:
                if (track != null)
                    track.LeftAt = evt.Timestamp;
                break;
        }

        await db.SaveChangesAsync();
    }
}