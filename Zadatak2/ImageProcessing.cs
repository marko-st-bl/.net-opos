using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace Zadatak2
{
    public class ImageProcessing
    {

        public enum ProcessingState { Pending, Downloading, Pausing, Paused, Resuming, Cancelling, Cancelled, Error, Done };

        public delegate void ProgressReportedDelegate(double progress, int downloadSpeed, ProcessingState processingState);

        public event ProgressReportedDelegate ProgressChanged;

        private Task processTask;
        private CancellationToken cancellationToken;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim pauseSemaphore = new SemaphoreSlim(1);

        public static async Task GrayscaleAsync(StorageFile file)
        {
            SoftwareBitmap softwareBitmap;

            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                Calculate(softwareBitmap);



                SemaphoreSlim semaphore = new SemaphoreSlim(1);

                /* for (int i = 0; i < softwareBitmap.PixelWidth; i++)
                 {
                     Parallel.For(0, softwareBitmap.PixelHeight, (y) =>
                     {
                         semaphore.Wait();

                     });
                 }*/

               
            }
        }

        private static unsafe void Calculate(SoftwareBitmap softwareBitmap)
        {
            using (BitmapBuffer buffer = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
            {
                using (var reference = buffer.CreateReference())
                {
                    byte* dataInBytes;
                    uint capacity;
                    ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacity);

                    // Fill-in the BGRA plane
                    BitmapPlaneDescription bufferLayout = buffer.GetPlaneDescription(0);
                    for (int i = 0; i < bufferLayout.Height; i++)
                    {
                        for (int j = 0; j < bufferLayout.Width; j++)
                        {

                            //byte value = (byte)((float)j / bufferLayout.Width * 255);
                            byte value = (byte)((
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 0] + 
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 1] + 
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 2]) 
                                / 3);
                            dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 0] = value;
                            dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 1] = value;
                            dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 2] = value;                          
                        }
                    }
                }
            }
            
            SaveSoftwareBitmapToFile(softwareBitmap);
        }

    private static async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap)
        {
            FileSavePicker fileSavePicker = new FileSavePicker();
            fileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileSavePicker.FileTypeChoices.Add("JPEG files", new List<string>() { ".jpg" });
            fileSavePicker.SuggestedFileName = "image";

            var outputFile = await fileSavePicker.PickSaveFileAsync();

            if (outputFile == null)
            {
                // The user cancelled the picking operation
                return;
            }

            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Create an encoder with the desired format
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                // Set the software bitmap
                encoder.SetSoftwareBitmap(softwareBitmap);

                // Set additional encoding parameters, if needed
                /*encoder.BitmapTransform.ScaledWidth = 320;
                encoder.BitmapTransform.ScaledHeight = 240;
                encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.Clockwise90Degrees;
                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;*/
                encoder.IsThumbnailGenerated = true;

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception err)
                {
                    const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
                    switch (err.HResult)
                    {
                        case WINCODEC_ERR_UNSUPPORTEDOPERATION:
                            // If the encoder does not support writing a thumbnail, then try again
                            // but disable thumbnail generation.
                            encoder.IsThumbnailGenerated = false;
                            break;
                        default:
                            throw;
                    }
                }

                if (encoder.IsThumbnailGenerated == false)
                {
                    await encoder.FlushAsync();
                }


            }
        }
    }
}
