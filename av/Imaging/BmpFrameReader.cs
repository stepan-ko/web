using web.av.AngelVisionLpr;

namespace web.av.Imaging;

internal static class BmpFrameReader
{
    public static BmpFrame Read(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);

        if (reader.ReadUInt16() != 0x4D42)
        {
            throw new InvalidDataException("Поддерживаются только BMP-файлы.");
        }

        reader.BaseStream.Position = 10;
        var pixelOffset = reader.ReadInt32();

        var dibHeaderSize = reader.ReadInt32();
        if (dibHeaderSize < 40)
        {
            throw new InvalidDataException("Поддерживается BMP с BITMAPINFOHEADER или совместимым DIB header.");
        }

        var width = reader.ReadInt32();
        var signedHeight = reader.ReadInt32();
        var planes = reader.ReadUInt16();
        var bitsPerPixel = reader.ReadUInt16();
        var compression = reader.ReadInt32();

        if (planes != 1 || compression != 0)
        {
            throw new InvalidDataException("Поддерживаются только несжатые BMP.");
        }

        if (bitsPerPixel is not 8 and not 24)
        {
            throw new InvalidDataException("Поддерживаются только 8-bit grayscale и 24-bit BGR BMP.");
        }

        var height = Math.Abs(signedHeight);
        var topDown = signedHeight < 0;
        var bytesPerPixel = bitsPerPixel / 8;
        var sourceStride = AlignTo4(width * bytesPerPixel);
        var targetStride = width * bytesPerPixel;
        var data = new byte[targetStride * height];

        reader.BaseStream.Position = pixelOffset;

        for (var sourceRow = 0; sourceRow < height; sourceRow++)
        {
            var targetRow = topDown ? sourceRow : height - 1 - sourceRow;
            var targetOffset = targetRow * targetStride;

            var row = reader.ReadBytes(sourceStride);
            if (row.Length != sourceStride)
            {
                throw new EndOfStreamException("BMP закончился раньше, чем ожидалось.");
            }

            Buffer.BlockCopy(row, 0, data, targetOffset, targetStride);
        }

        var format = bitsPerPixel == 24 ? AitChannelFormat.U8C3 : AitChannelFormat.U8C1;
        return new BmpFrame(width, height, format, data, targetStride);
    }

    private static int AlignTo4(int value)
    {
        return (value + 3) / 4 * 4;
    }

    
}
