using System.Runtime.InteropServices;

namespace web.av.AngelVisionLpr;

internal static class PlateNativeMemory
{
    public static void ReleaseOwnedBuffers(AitAvLicensePlate plate)
    {
        // SDK может вернуть лучший кадр номера в unmanaged-памяти.
        // После копирования/использования эту память должен освободить вызывающий код.
        if (plate.BestImage.Data != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(plate.BestImage.Data);
        }

        var track = plate.Track;
        PlateBuffer.Free(ref track);
    }
}
