public class FileImageStorage : IImageStorage
{
    public async Task<string> SavePlateAsync(byte[] jpg, string plate, int cameraId)
    {        
        var now = DateTime.UtcNow;

        var dir = Path.Combine(
            "PlateImages",            
            now.Year.ToString(),
            now.Month.ToString("D2"),
            now.Day.ToString("D2")
        );

        Directory.CreateDirectory(dir);
        var fileName = $"{cameraId}_{plate}_{now:HHmmss}.jpg";
        var fullPath = Path.Combine(dir, fileName);
        
        await File.WriteAllBytesAsync(fullPath, jpg);
        return fullPath;
    }
}