using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

public class TrackService : ITrackService
{
    
    private readonly ILogger<TrackService> _logger;
    private readonly AppDbContext _db;

    public TrackService(AppDbContext db, ILogger<TrackService> logger)
    {       
        _logger = logger;
        _db = db;
    }
    
    public async Task<TrackRecognize> StartAsync(int cameraId, string plate, double probability)
    {       
        var track = new TrackRecognize
        {
            CameraId = cameraId,
            PlateNumber = plate,
            FirstSeen = DateTime.Now,
            LastSeen = DateTime.Now,
            BestProbability = probability
        };

        _db.Add(track);
        await _db.SaveChangesAsync();
        return track;        
    }

    public async Task UpdateAsync(long trackId, double probability)
    {
       
        var track = await _db.Set<TrackRecognize>()
            .FirstOrDefaultAsync(x => x.Id == trackId);

        if (track == null)
            return;

        track.LastSeen = DateTime.Now;

        if (probability > track.BestProbability)
            track.BestProbability = probability;
        
        await _db.SaveChangesAsync();
    }

    public async Task CloseAsync(long trackId)
    {
       var track = await _db.Set<TrackRecognize>()
                            .FirstOrDefaultAsync(x => x.Id == trackId);

        if (track == null)
            return;

        track.LeftAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    public async Task<TrackRecognize?> GetActiveAsync(int cameraId)
    {  
        return await _db.Set<TrackRecognize>()
            .Where(x => x.CameraId == cameraId && x.LeftAt == null)
            .OrderByDescending(x => x.LastSeen)
            .FirstOrDefaultAsync();
    }

}