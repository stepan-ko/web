using System.Runtime.InteropServices;

namespace web.av.AngelVisionLpr;

[StructLayout(LayoutKind.Sequential)]
internal struct AitRect
{
    public int X;
    public int Y;
    public int Width;
    public int Height;

    public AitRect(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}
