using DotNetFFmpegPipe.Common;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace DotNetFFmpegPipe.SimplePipe2
{
    class Program
    {
        /// <summary>
        /// Overlay an existing Video with transparent pngs
        /// </summary>
        static void Main()
        {
            var ffmpeg = "ffmpeg.exe";
            if (!File.Exists(ffmpeg))
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                File.Copy(@"C:\Temp\ffmpeg.exe", Path.Combine(currentDirectory, ffmpeg));
            }

            var fps = 10;
            var duration = 60;
            var frameCount = fps * duration;

            Console.WriteLine("Start");
            var sw = new Stopwatch();
            sw.Start();

            //The performance is much better if it is not a full overlay

            var inputArgs = $"-y -i myvideo.mp4 -framerate 10 -i -";
            var outputArgs = "-filter_complex \"[0:0][1:0]overlay=0:120[out]\" -map [out] -vcodec libx264 -crf 23 -pix_fmt yuv420p -preset ultrafast -movflags +faststart -r 20 out.mp4";

            var process = new Process
            {
                StartInfo =
                {
                    FileName = ffmpeg,
                    Arguments = $"{inputArgs} {outputArgs}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                },
            };

            process.ErrorDataReceived += (sender, eventArgs) => Console.WriteLine(eventArgs.Data);
            process.Start();
            process.BeginErrorReadLine();

            var ffmpegIn = process.StandardInput.BaseStream;

            using (var image = new Bitmap(1920, 50))
            using (var canvas = Graphics.FromImage(image))
            {
                for (var i = 0; i < frameCount; i++)
                {
                    var percent = (double)i / frameCount;

                    var backgroundColor = Color.Transparent;
                    canvas.Clear(backgroundColor);

                    canvas.FillRectangle(Brushes.White, i, 0, 50, 50);
                    canvas.Flush();

                    // Draw your image
                    var imageData = image.GetBytes(ImageFormat.Png);

                    // Write Data
                    ffmpegIn.Write(imageData, 0, imageData.Length);
                }
            }

            Console.WriteLine("Drawing done");

            ffmpegIn.Flush();
            ffmpegIn.Close();

            process.WaitForExit();
            process.Dispose();

            sw.Stop();
            Console.WriteLine($"Video creating done {sw.Elapsed.TotalSeconds:0.00}s");
            Console.ReadLine();
        }
    }
}
