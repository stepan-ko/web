namespace AngelVisionLprDemo.AngelVisionLpr;

internal sealed class LprRecognizer : IDisposable
{
    private IntPtr _handle;

    public LprRecognizer(AitAvOptions options)
    {
        _handle = NativeMethods.RecognizerAlloc(options);

        if (_handle == IntPtr.Zero)
        {
            throw new InvalidOperationException("av_lpr_recognizer_alloc вернул null.");
        }
    }

    public PlateBuffer Recognize(AitImage image, ulong frameId, string info)
    {
        ThrowIfDisposed();

        var error = NativeMethods.Recognize(_handle, image, out var plateBuffer, frameId, info);

        return error switch
        {
            AitError.None => new PlateBuffer(plateBuffer),
            AitError.ArgumentIsNull => throw new ArgumentNullException(nameof(image), "Native API вернул ArgumentIsNull."),
            AitError.ArgumentIsInvalid => throw new ArgumentException("Native API вернул ArgumentIsInvalid.", nameof(image)),
            AitError.CalleeNull => throw new InvalidOperationException("Native API вернул CalleeNull: recognizer не создан."),
            _ => throw new InvalidOperationException($"Native API вернул неизвестную ошибку: {error}.")
        };
    }

    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
        {
            NativeMethods.RecognizerFree(ref _handle);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_handle == IntPtr.Zero)
        {
            throw new ObjectDisposedException(nameof(LprRecognizer));
        }
    }
}
