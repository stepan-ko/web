using web.av.AngelVisionLpr;

namespace web.av.Imaging;

internal sealed class BmpFrame : IDisposable
{
    public BmpFrame(int width, int height, AitChannelFormat format, byte[] data, int stride)
    {
        Width = width;
        Height = height;
        Format = format;
        Data = data;
        Stride = stride;
    }

    public int Width { get; }
    public int Height { get; }
    public AitChannelFormat Format { get; }
    public byte[] Data { get; }
    public int Stride { get; }

    public void Dispose()
    {
    }
}
