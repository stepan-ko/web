public class CameraViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string StreamUrl { get; set; } = "";
    public bool Simulate { get; set; }
    public bool Enable { get; set; }
    public CameraOptionViewModel Option { get; set; } = new();
}