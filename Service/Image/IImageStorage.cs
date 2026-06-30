public interface IImageStorage
{
    Task<string> SavePlateAsync(byte[] jpg, string plate, int cameraId);
    Task<string> SaveFrameAsync(byte[] jpg, string plate, int cameraId);
}