using Microsoft.EntityFrameworkCore;
using OpenCvSharp;
using System.Collections.Concurrent;
using System.Diagnostics;

public class PlateAnalyse
{
    
    private Dictionary<string, PlateDetect> _activeTracks = new Dictionary<string, PlateDetect>();
    public IPlateEventService _events;
    private readonly ILogger<PlateAnalyse> _logger;
    private int CameraId;
    private int countFrameDetect ;
    private int timeLost;

    public PlateAnalyse(ILogger<PlateAnalyse> logger, IPlateEventService events)
    {
        _logger = logger;
        _events = events;
    }


public void Init(Camera camera)
    {
        
        CameraId = camera.Id;
        timeLost = camera.TimeLost;
        countFrameDetect = camera.Fps * camera.TimeDetect;
    }

public void Detect(string plateNumber)
    {
        string key = plateNumber;
        
        if (!ValidNumber(key))
        return;
        
                
        if (_activeTracks.ContainsKey(key))        
        {
            // Номер уже был, обновляем данные 
            _activeTracks[key].LastDetect = DateTime.Now;           

            if (!_activeTracks[key].IsActive && ++_activeTracks[key].CountFrame > countFrameDetect)
            {
                // Значит Определаем что он АКТИВНЫЙЙ
               _activeTracks[key].IsActive = true;
               _logger.LogDebug(key + ", ОБНАРУЖЕН - " + DateTime.Now);    

                // _events.Raise(new PlateEvent
                // {
                //     CameraId = CameraId,
                //     PlateNumber = key,
                //     Type = PlateEventType.Active,
                //     Timestamp = DateTime.Now
                // });  
            }
            return;
        }
        
        //  номера нет, добавляем
        var plateDetect = new PlateDetect
        {
            CountFrame = 1,
            FirstDetect = DateTime.Now,
            LastDetect = DateTime.Now,
            IsActive = false
        };
        _activeTracks.Add(key,plateDetect);
        _logger.LogDebug(key + ", ПЕРВЫЙ КАДР в = " + _activeTracks[key].FirstDetect);
        
        // _events.Raise(new PlateEvent
        // {
        //     CameraId = CameraId,
        //     PlateNumber = key,
        //     Type = PlateEventType.Detect,
        //     Timestamp = DateTime.Now
        // });

    }


    public void Lost()
    {       
        var timeNow = DateTime.Now; 
        //Запуск/Перезапуск таймера для определения что номер покинул кадр
        foreach (var track in _activeTracks)
        {
            var timeDiff = timeNow - track.Value.LastDetect;

            if (timeDiff > TimeSpan.FromSeconds(timeLost))
            {                
                _logger.LogDebug($"{track.Key} ПОКИНУЛ КАДР в {timeNow} , разница времени {timeDiff}, последний раз в {track.Value.LastDetect}");

                // _events.Raise(new PlateEvent
                // {
                //     CameraId = CameraId,
                //     PlateNumber = track.Key,
                //     Type = PlateEventType.Lost,
                //     Timestamp = timeNow
                // });

                _activeTracks.Remove(track.Key);
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