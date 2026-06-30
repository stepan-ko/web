using Microsoft.EntityFrameworkCore;
public class PlateEventWorker : BackgroundService
{
    private readonly IPlateEventService _service;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PlateEventWorker> _logger;
    private readonly IImageStorage _imageStorage;

    public PlateEventWorker(ILogger<PlateEventWorker> logger,
        IPlateEventService service,
        IServiceScopeFactory scopeFactory,
        IImageStorage imageStorage)
    {
        _logger = logger;
        _service = service;
        _scopeFactory = scopeFactory;
        _imageStorage = imageStorage;
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
                        LastSeen = evt.Timestamp,
                        BestProbability = evt.BestProbability,
                    });
                }
                break;

            case PlateEventType.Active:
                if (track != null)
                {
                    track.LastSeen = evt.Timestamp;
                    track.BestProbability = evt.BestProbability;
                }
                    
                break;

            case PlateEventType.Lost:
                if (track != null)
                {
                    track.LeftAt = evt.Timestamp;
                    track.BestProbability = evt.BestProbability;
                    
                     if (evt.BestImageBytes != null)
                    {
                        var path = await _imageStorage.SavePlateAsync(
                            evt.BestImageBytes,
                            track.PlateNumber,
                            track.CameraId
                        );

                        track.BestImagePath = path;
                    }                  
                }                    
                break;
        }

     

        await db.SaveChangesAsync();
    }
}