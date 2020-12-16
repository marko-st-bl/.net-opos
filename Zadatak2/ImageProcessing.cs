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

        public enum ProcessingState { Pending, Processing, Pausing, Paused, Resuming, Cancelling, Cancelled, Error, Done };

        public delegate void ProgressReportedDelegate(double progress, ProcessingState processingState);

        public event ProgressReportedDelegate ProgressChanged;

        private Task processTask;
        private CancellationTokenSource cancellationTokenSource;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim pauseSemaphore = new SemaphoreSlim(1);

        public ProcessingState CurrentState { get; private set; } = ProcessingState.Pending;
        public bool IsFinished => CurrentState == ProcessingState.Done || CurrentState == ProcessingState.Error || CurrentState == ProcessingState.Cancelled;
        public bool IsInitialized { get; private set; }
        public bool IsPending => CurrentState == ProcessingState.Pending;
        public string Filename { get; private set; }

        private StorageFile outputFile;
        private StorageFile sourceFile;

        public ImageProcessing(StorageFile sourceFile, StorageFile outputFile)
        {
            this.sourceFile = sourceFile;
            this.outputFile = outputFile;
            this.Filename = sourceFile.Name;
            this.IsInitialized = true;
        }

        public ImageProcessing(StorageFile sourceFile)
        {
            this.sourceFile = sourceFile;
            this.Filename = sourceFile.Name;
        }

        public void Initialize(StorageFile outputFile)
        {
            this.outputFile = outputFile;
            IsInitialized = true;
            CurrentState = ProcessingState.Pending;
        }

        public async Task GrayscaleAsync(CancellationToken cancellationToken)
        {
            CurrentState = ProcessingState.Processing;
            ProgressChanged?.Invoke(0, CurrentState);

            SoftwareBitmap softwareBitmap;

            using (IRandomAccessStream stream = await sourceFile.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                Calculate(softwareBitmap);               
            }
        }

        private unsafe async void Calculate(SoftwareBitmap softwareBitmap)
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
                            if(CurrentState == ProcessingState.Pausing)
                            {
                                CurrentState = ProcessingState.Paused;
                                ProgressChanged?.Invoke((double)i/bufferLayout.Height, CurrentState);
                                //TRY TO CHANGE TO WAITASZNC()
                                pauseSemaphore.Wait();
                                pauseSemaphore.Release();
                                CurrentState = ProcessingState.Processing;
                            }
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
                        ProgressChanged?.Invoke(i / bufferLayout.Height, ProcessingState.Processing);
                    }

                    ProgressChanged?.Invoke(1.0, ProcessingState.Done);
                }
            }
            
            SaveSoftwareBitmapToFile(softwareBitmap);
        }

    private async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap)
        {
            /*FileSavePicker fileSavePicker = new FileSavePicker();
            fileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileSavePicker.FileTypeChoices.Add("JPEG files", new List<string>() { ".jpg" });
            fileSavePicker.SuggestedFileName = "image";

            var outputFile = await fileSavePicker.PickSaveFileAsync();*/

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

        public async Task Start(bool silent = false)
        {
            await semaphore.WaitAsync();
            try
            {
                if (CurrentState == ProcessingState.Pending || !IsInitialized)
                {
                    cancellationTokenSource = new CancellationTokenSource();
                    processTask = Task.Factory.StartNew(async () => await GrayscaleAsync(cancellationTokenSource.Token), cancellationTokenSource.Token);
                }
                else if (!silent)
                    throw new InvalidOperationException("The task is already started.");
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task Reset()
        {
            await semaphore.WaitAsync();
            try
            {
                if (IsFinished)
                    CurrentState = ProcessingState.Pending;
                else
                    throw new InvalidOperationException("Cannot reset an active task.");
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task Cancel(bool silent = false)
        {
            await semaphore.WaitAsync();
            try
            {
                if (CurrentState == ProcessingState.Pending)
                    CurrentState = ProcessingState.Cancelled;
                else if (CurrentState == ProcessingState.Processing || CurrentState == ProcessingState.Pausing || CurrentState == ProcessingState.Paused || CurrentState == ProcessingState.Pending)
                {
                    CurrentState = ProcessingState.Cancelling;
                    cancellationTokenSource.Cancel();
                }
                else if (!silent)
                    throw new InvalidOperationException("The task cannot be cancelled.");
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task Pause()
        {
            await semaphore.WaitAsync();
            try
            {
                if (CurrentState == ProcessingState.Processing)
                {
                    CurrentState = ProcessingState.Pausing;
                    await pauseSemaphore.WaitAsync();
                }
                else
                    throw new InvalidOperationException("Only a downloading task can be paused.");
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task Resume()
        {
            await semaphore.WaitAsync();
            try
            {
                if (CurrentState == ProcessingState.Paused || CurrentState == ProcessingState.Pausing)
                {
                    CurrentState = ProcessingState.Resuming;
                    pauseSemaphore.Release();
                }
                else
                    throw new InvalidOperationException("Only a downloading task can be paused.");
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
