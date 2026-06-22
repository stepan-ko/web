public interface ITrackService
{
    Task<TrackRecognize> StartAsync(int cameraId, string plate, double probability);
    
    Task UpdateAsync(long trackId, double probability);

    Task CloseAsync(long trackId);

    Task<TrackRecognize?> GetActiveAsync(int cameraId);
}