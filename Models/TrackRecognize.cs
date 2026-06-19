public class TrackRecognize
{
    public long Id { get; set; }

    public int CameraId { get; set; }
    public Camera Camera { get; set; } = null!;

    public string PlateNumber { get; set; } = "";

    // когда впервые увидели
    public DateTime FirstSeen { get; set; }

    // когда последний раз увидели
    public DateTime LastSeen { get; set; }

    // машина покинула кадр
    public DateTime? LeftAt { get; set; }

    // максимальная вероятность за всю сессию
    public double BestProbability { get; set; }

    // лучший кадр
    public string? BestImagePath { get; set; }

}