namespace AngelVisionLpr;

internal sealed class PlateBuffer : IDisposable
{
    private IntPtr _handle;

    public PlateBuffer(IntPtr handle)
    {
        _handle = handle;
    }

    public IEnumerable<AitAvLicensePlate> PopAll()
    {
        while (_handle != IntPtr.Zero && NativeMethods.PlateBufferPop(ref _handle, out var plate) != 0)
        {
            yield return plate;
        }
    }

    public void Dispose()
    {
        Free(ref _handle);
    }

    public static void Free(ref IntPtr handle)
    {
        if (handle != IntPtr.Zero)
        {
            NativeMethods.PlateBufferFree(ref handle);
        }
    }
}
