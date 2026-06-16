using System.Collections.Concurrent;
public class FrameBuffer
{
    private readonly ConcurrentDictionary<int, byte[]> _frames = new();

    public void SetFrame(int cameraId, byte[] frame)
    {
        _frames[cameraId] = frame;
    }

    public byte[]? GetFrame(int cameraId)
    {
        return _frames.TryGetValue(cameraId, out var frame)
            ? frame
            : null;
    }
}