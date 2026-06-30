public class PlateDetect
{
    public int CountFrame { get; set; }
    public DateTime FirstDetect { get; set; } 
    public DateTime? LastDetect { get; set; } 
    public bool IsActive { get; set; } 
    public double BestProbability { get; set; }
    public byte[]? BestImageBytes { get; set; }
    public byte[]? BestFrameBytes { get; set; }
    public void UpdateBest(double nowProb, byte[]? bytesImage, byte[]? bytesFrame)
    {
        if (nowProb > BestProbability)
        {
            BestProbability = nowProb;
            BestImageBytes = bytesImage;
            BestFrameBytes = bytesFrame;
        }
    }
}
   