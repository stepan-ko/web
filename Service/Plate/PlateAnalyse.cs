using Microsoft.EntityFrameworkCore;
using OpenCvSharp;
using System.Collections.Concurrent;
using System.Diagnostics;

public class PlateAnalyse
{
    
    private Dictionary<string, PlateDetect> _activeTracks = new Dictionary<string, PlateDetect>();

    private int countTracks;
    private readonly ILogger<CameraManager> logger;
    public PlateAnalyse(ILogger<CameraManager> _logger)
    {
        logger = _logger;
    }

private int countFrameDetect = 15;
private int timeLost = 40;

public void Detect(TrackActive track)
    {
        if (!ValidNumber(track.PlateNumber))
        return;
        
        string key = track.PlateNumber;        
        if (_activeTracks.ContainsKey(key) && !_activeTracks[key].IsActive)        
        {
            // Номер уже был, обновляем данные 
            _activeTracks[key].LastDetect = DateTime.Now;

            if (++_activeTracks[key].CountFrame > countFrameDetect)
            {
                // Значит Определаем что он АКТИВНЫЙЙ и удаляем со словаря
                logger.LogDebug(key + ", ОБНАРУЖЕН - " + DateTime.Now);                
                _activeTracks[key].IsActive = true;
            }

        }
         
        if (!_activeTracks.ContainsKey(key))
        {
            //  номера нет, добавляем
            var plateDetect = new PlateDetect
            {
                CountFrame = 1,
                FirstDetect = DateTime.Now,
                LastDetect = DateTime.Now,
                IsActive = false
            };
            _activeTracks.Add(key,plateDetect);
            logger.LogDebug(key + ", ПЕРВЫЙ КАДР в = " + _activeTracks[key].FirstDetect);
            
        }
        
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
                if (track.Value.IsActive)
                {
                    logger.LogDebug($"{track.Key} ПОКИНУЛ КАДР в {timeNow} , разница времени {timeDiff}, последний раз в {track.Value.LastDetect}");
                    
                }
                else
                {
                   logger.LogDebug($"{track.Key} УДАЛЕН без Активности {timeNow} , разница времени {timeDiff}, последний раз в {track.Value.LastDetect}"); 
                }
                _activeTracks.Remove(track.Key, out _);
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