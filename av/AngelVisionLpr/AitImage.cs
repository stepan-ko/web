using System.Runtime.InteropServices;

namespace AngelVisionLpr;

[StructLayout(LayoutKind.Sequential)]
internal struct AitImage
{
    public int Width;
    public int Height;

    // U8C1 - grayscale, U8C3 - три 8-битных канала.
    public AitChannelFormat Format;

    public int DataSize;

    // Указатель должен быть валиден на время вызова av_lpr_recognize.
    public IntPtr Data;

    // Количество байт между началами соседних строк изображения.
    public int Stride;
}
