using web.av.AngelVisionLpr;
using System.Runtime.InteropServices;

namespace web.av.Imaging;

internal sealed class PinnedFrame : IDisposable
{
    private readonly GCHandle _handle;

    private PinnedFrame(BmpFrame frame)
    {
        _handle = GCHandle.Alloc(frame.Data, GCHandleType.Pinned);

        Image = new AitImage
        {
            Width = frame.Width,
            Height = frame.Height,
            Format = frame.Format,
            DataSize = frame.Data.Length,
            Data = _handle.AddrOfPinnedObject(),
            Stride = frame.Stride
        };
    }

    public AitImage Image { get; }

    public static PinnedFrame Pin(BmpFrame frame)
    {
        return new PinnedFrame(frame);
    }

    public void Dispose()
    {
        if (_handle.IsAllocated)
        {
            _handle.Free();
        }
    }
}
