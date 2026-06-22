using Microsoft.EntityFrameworkCore;
using OpenCvSharp;
using System.Collections.Concurrent;
using System.Diagnostics;

public class PlateAnalyse
{
    
    private ConcurrentDictionary<string, TrackActive> _activeTracks = new ConcurrentDictionary<string, TrackActive>();

    private int countTracks;
    private readonly ILogger<CameraManager> logger;
    public PlateAnalyse(ILogger<CameraManager> _logger)
    {
        logger = _logger;
    }
     public void CheckTrack(TrackActive track)
    {
         _activeTracks.AddOrUpdate(
        track.PlateNumber,
        _ => track,
        (_, existing) =>
        {
            existing.LastSeen = DateTime.Now;
            existing.BestProbability = track.BestProbability;
            return existing;
        });
       
       if (_activeTracks.Count > countTracks)
        {
            countTracks = _activeTracks.Count;
            logger.LogTrace($"Количество _activeTracks = {countTracks}");
            foreach (var t in _activeTracks)
            {
                logger.LogTrace(t.Value.PlateNumber);
            }
            
            
        }
      
    } 

}