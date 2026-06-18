using System.Runtime.InteropServices;

namespace web.av.AngelVisionLpr;

[StructLayout(LayoutKind.Sequential)]
internal struct AitAvPlateData
{
    public AitRect Position;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
    public string PlateText;

    public uint Identifier;
    public float Probability;
}
