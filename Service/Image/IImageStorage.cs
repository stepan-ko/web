public interface IImageStorage
{
    Task<string> SavePlateAsync(byte[] jpg, string plate, int cameraId);
}