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
        if (!ValidNumber(track.PlateNumber))
        return;



        _activeTracks.AddOrUpdate(
        track.PlateNumber,
        _ => track,
        (_, existing) =>
        {
            existing.LastSeen = DateTime.Now;
            existing.BestProbability = track.BestProbability;
            ++existing.CountFrame;
            return existing;
        });
       
     logger.LogTrace(track.PlateNumber + " , " + track.BestProbability +" , " + track.CountFrame + " , " + track.RectPlate.X + " , " + track.RectPlate.Y + " , " + track.RectPlate.Width + " , " + track.RectPlate.Height);

       if (_activeTracks.Count > countTracks)
        {
            countTracks = _activeTracks.Count;
            logger.LogTrace($"Количество _activeTracks = {countTracks}");
            foreach (var t in _activeTracks)
            {
                logger.LogTrace(t.Value.PlateNumber + " , " + t.Value.CountFrame + " , " + t.Value.RectPlate.X + " , " + t.Value.RectPlate.Y + " , " + t.Value.RectPlate.Width + " , " + t.Value.RectPlate.Height);
            }
            
            
        }
      
    } 




    public static bool ValidNumber(string number)
        {
            if (number.Length >= 8 && isLetterChar(number[0]) && char.IsNumber(number[1]) && char.IsNumber(number[2]) && char.IsNumber(number[3]) && isLetterChar(number[4]) && isLetterChar(number[5]) && char.IsNumber(number[6]) && char.IsNumber(number[7]))
            {
                if (number.Length == 8) return true;
                if (number.Length == 9 && char.IsNumber(number[8])) return true;
            }

            return false;
        }

    public static bool isLetterChar(char ch)
    {
        if (ch == 'A' || ch == 'B' || ch == 'E' || ch == 'K' || ch == 'M' || ch == 'H' ||
            ch == 'O' || ch == 'P' || ch == 'C' || ch == 'T' || ch == 'X' || ch == 'Y')
        {
            return true;
        }
        return false;
    }

}