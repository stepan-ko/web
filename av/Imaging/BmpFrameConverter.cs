using web.av.AngelVisionLpr;
using OpenCvSharp;
using System.Runtime.InteropServices;

namespace web.av.Imaging;

internal static class BmpFrameConverter
{
    public static BmpFrame FromMat(Mat mat)
    {
        int width = mat.Width;
        int height = mat.Height;
        int channels = mat.Channels();

        int stride = width * channels;
        byte[] data = new byte[stride * height];

        if (mat.IsContinuous())
        {
            Marshal.Copy(mat.Data, data, 0, data.Length);
        }
        else
        {
            for (int y = 0; y < height; y++)
            {
                IntPtr src = mat.Ptr(y);
                Marshal.Copy(src, data, y * stride, stride);
            }
        }

        var format = channels == 3
            ? AitChannelFormat.U8C3
            : AitChannelFormat.U8C1;

        return new BmpFrame(width, height, format, data, stride);
    }
}