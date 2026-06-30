using OpenCvSharp;

public class PlateResult
{
    public Rect RectPlate { get; set; }

    public uint TrackerId { get; set; }

    public string PlateNumber { get; set; } = "";

    public double Probability { get; set; }

    public byte[]? BestImageBytes { get; set; }

    public byte[]? BestFrameBytes { get; set; }

}