using System.Runtime.InteropServices;

namespace web.av.AngelVisionLpr;

[StructLayout(LayoutKind.Sequential)]
internal struct AitAvLicensePlate
{
    public AitAvPlateState State;
    public AitAvPlateData Data;

    // SDK может вернуть лучший crop номера в unmanaged-памяти.
    public AitImage BestImage;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1000)]
    public string BestImageInfo;

    public ulong FrameId;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
    public string Country;

    // История/трек объекта приходит отдельным native buffer-ом.
    public IntPtr Track;
}
