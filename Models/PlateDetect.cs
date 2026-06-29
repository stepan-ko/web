public class PlateDetect
{
    public int CountFrame { get; set; }
    public DateTime FirstDetect { get; set; } 
    public DateTime? LastDetect { get; set; } 
    public bool IsActive { get; set; } 
    public double BestProbability { get; set; }
    public byte[]? BestImageBytes { get; set; }
}
   