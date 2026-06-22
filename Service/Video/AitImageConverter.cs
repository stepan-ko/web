using OpenCvSharp;
namespace web.av.AngelVisionLpr;

internal static class AitImageConverter
{
    internal  static byte[] ToJpeg(AitImage image)
    {
        if (image.Data == IntPtr.Zero)
            return Array.Empty<byte>();

        var matType = image.Format switch
        {
            AitChannelFormat.U8C1 => MatType.CV_8UC1,
            AitChannelFormat.U8C3 => MatType.CV_8UC3,
            _ => throw new NotSupportedException(
                $"Format {image.Format} not supported")
        };

        using var mat = Mat.FromPixelData(
            image.Height,
            image.Width,
            matType,
            image.Data,
            image.Stride);

        Cv2.ImEncode(
            ".jpg",
            mat,
            out byte[] jpg,
            new[] { (int)ImwriteFlags.JpegQuality, 90 });

        return jpg;
    }
}