using System.Security.Cryptography.X509Certificates;

public enum PlateEventType
{
    Detect,
    Active,
    Lost
}

public class PlateEvent
{
    public int CameraId { get; set; }
    public string PlateNumber { get; set; } = "";
    public PlateEventType Type { get; set; }

    public DateTime Timestamp { get; set; }
      
    public double BestProbability { get; set; }

    public byte[]? BestImageBytes { get; set; }
    
}