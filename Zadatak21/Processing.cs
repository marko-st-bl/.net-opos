using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Zadatak21
{
    public class Processing
    {
        static SemaphoreSlim semaphore = new SemaphoreSlim(1);
        public static void Grayscale(string path)
        {
            Bitmap original = new Bitmap(path);
            Bitmap grayscale = new Bitmap(original.Width, original.Height);

            /* Parallel.For(0, original.Width, (x, width) =>
             {*/
            for (int x = 0; x < original.Width; x++)
            {
                Parallel.For(0, original.Height, (y, height) =>
                {
                    semaphore.Wait();
                    Color pixel = original.GetPixel(x, y);
                    int a = pixel.A;
                    int R = pixel.R;
                    int G = pixel.G;
                    int B = pixel.B;

                    int avg = (R + G + B) / 3;
                    grayscale.SetPixel(x, y, Color.FromArgb(a, avg, avg, avg));
                    semaphore.Release();

                    //Console.WriteLine(avg);
                });
            }
            Console.WriteLine("DONE");
            //});
            grayscale.Save(@"c:\gray.bmp");
        }
    }
}
