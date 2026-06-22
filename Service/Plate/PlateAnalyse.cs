using Microsoft.EntityFrameworkCore;
using OpenCvSharp;
using System.Collections.Concurrent;
using System.Diagnostics;

public class PlateAnalyse
{
    
    private ConcurrentDictionary<string, TrackActive> _activeTracks;


    public void CheckTrack(TrackActive track)
    {
        string key = track.PlateNumber;
        
        if (!_activeTracks.ContainsKey(key))
        {            
            //Не было в камере, нужно добавить
            _activeTracks.TryAdd(key, track);
            Debug.WriteLine($"TryAdd({key}, {track})");
        }
        else
        {
            //Уже был, необходимо обновить 
            _activeTracks[key].LastSeen = DateTime.Now;
            _activeTracks[key].BestProbability = track.BestProbability;       
             Debug.WriteLine($"Update LastSeen = {_activeTracks[key].LastSeen}, BestProbability = {_activeTracks[key].BestProbability})");     
        }
        
        
    } 

}