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

private int countFrameDetect = 10;

public void Analize(TrackActive track)
    {
        if (!ValidNumber(track.PlateNumber))
        return;
        
        string key = track.PlateNumber;        
        if (_activeTracks.ContainsKey(key) && !_activeTracks[key].isActive)        
        {
            // Номер уже был, обновляем данные 
            if (++_activeTracks[key].countFrame > countFrameDetect)
            {
                // Значит Определаем что он АКТИВНЫЙЙ и удаляем со словаря
                logger.LogDebug(key + ", ОБНАРУЖЕН = " + DateTime.Now);
                _activeTracks.Remove(key);
                _activeTracks[key].isActive = true;
            }
        }
         
        if (!_activeTracks.ContainsKey(key))
        {
            //  номера нет, добавляем
            var plateDetect = new PlateDetect
            {
                countFrame = 1,
                firstDetect = DateTime.Now,
                isActive = false
            };
            _activeTracks.Add(key,plateDetect);
            logger.LogDebug(key + ", ПЕРВЫЙ КАДР в = " + _activeTracks[key].firstDetect);
            // Task.Run(async () =>
            // {
            //     await Task.Delay(5000);
            //     logger.LogDebug(key + ", probability= " + _probability[key]);
            // });
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