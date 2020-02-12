using DotNetFFmpegPipe.Common;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace DotNetFFmpegPipe.SimplePipe1
{
    class Program
    {
        static void Main()
        {
            var ffmpeg = "ffmpeg.exe";
            if (!File.Exists(ffmpeg))
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                File.Copy(@"C:\Temp\ffmpeg.exe", Path.Combine(currentDirectory, ffmpeg));
            }

            var fps = 20;
            var duration = 60;
            var frameCount = fps * duration;

            Console.WriteLine("Create a video with a color fade and a moving circle");
            var sw = new Stopwatch();
            sw.Start();

            var inputArgs = $"-y -framerate {fps} -f image2pipe -i -";
            var outputArgs = "-vcodec libx264 -crf 23 -pix_fmt yuv420p -preset ultrafast -r 20 out.mp4";

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

            using (var image = new Bitmap(1920, 1080))
            using (var canvas = Graphics.FromImage(image))
            {
                for (var i = 0; i < frameCount; i++)
                {
                    var percent = (double)i / frameCount;

                    var backgroundColor = Color.Yellow.Interpolate(Color.Green, percent);
                    canvas.Clear(backgroundColor);

                    canvas.FillPie(Brushes.White, i, 50, 50, 50, 0, 360);
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
