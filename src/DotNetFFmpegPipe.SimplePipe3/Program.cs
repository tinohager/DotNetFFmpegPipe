using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DotNetFFmpegPipe.SimplePipe3
{
    class Program
    {
        /// <summary>
        /// Read frames from a video
        /// </summary>
        static void Main()
        {
            var ffmpeg = "ffmpeg.exe";
            if (!File.Exists(ffmpeg))
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                File.Copy(@"C:\Temp\ffmpeg.exe", Path.Combine(currentDirectory, ffmpeg));
            }

            Console.WriteLine("Start");
            var sw = new Stopwatch();
            sw.Start();

            var outputFormat = "bmp";

            var inputArgs = $"-y -i myvideo.mp4";
            var outputArgs = $"-frames 100 -c:v {outputFormat} -f image2pipe -";

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
                    RedirectStandardOutput = true,
                },
            };

            process.ErrorDataReceived += (sender, eventArgs) => Console.WriteLine(eventArgs.Data);
            process.Start();
            process.BeginErrorReadLine();

            Stream output = process.StandardOutput.BaseStream;

            var index = 0;
            var buffer = new byte[32768];
            var imageData = new List<byte>();
            byte[] imageHeader = null;

            while (true)
            {
                var length = output.Read(buffer, 0, buffer.Length);
                if (length == 0)
                {
                    break;
                }

                if (imageHeader == null)
                {
                    imageHeader = buffer.Take(5).ToArray();
                }

                if (buffer.Take(5).SequenceEqual(imageHeader))
                {
                    if (imageData.Count > 0)
                    {
                        File.WriteAllBytes($"image{index}.{outputFormat}", imageData.ToArray());
                        imageData.Clear();
                        index++;
                    }
                }
               
                imageData.AddRange(buffer.Take(length));                
            }

            File.WriteAllBytes($"image{index}.{outputFormat}", imageData.ToArray());

            output.Close();
            output.Dispose();

            process.WaitForExit();
            process.Dispose();

            sw.Stop();
            Console.WriteLine($"Video creating done {sw.Elapsed.TotalSeconds:0.00}s");
            Console.ReadLine();
        }
    }
}
