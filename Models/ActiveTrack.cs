
public class ActiveTrack
{    
    public Guid TrackId { get; set; }
    public int CameraId { get; set; }
    
    public string PlateNumber { get; set; } = "";

    // когда впервые увидели
    public DateTime FirstSeen { get; set; }

    // когда последний раз увидели
    public DateTime LastSeen { get; set; }
 
    // максимальная вероятность за всю сессию
    public double BestProbability { get; set; }

    // лучший кадр
    public byte[]? BestImageBytes { get; set; }

    // внутренний id трекера SDK
    public uint TrackerId { get; set; }

}