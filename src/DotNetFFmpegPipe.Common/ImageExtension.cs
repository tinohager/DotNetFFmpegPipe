using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace DotNetFFmpegPipe.Common
{
    public static class ImageExtension
    {
        public static byte[] GetBytes(this Image imgage, ImageFormat imageFormat)
        {
            using (var stream = new MemoryStream())
            {
                imgage.Save(stream, imageFormat);
                return stream.ToArray();
            }
        }
    }
}
