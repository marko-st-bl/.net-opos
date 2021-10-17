using System;
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

        public ProcessingState CurrentState { get; set; } = ProcessingState.Pending;
        public bool IsFinished => CurrentState == ProcessingState.Done || CurrentState == ProcessingState.Error || CurrentState == ProcessingState.Cancelled;
        public bool IsInitialized { get; set; }
        public bool IsPending => CurrentState == ProcessingState.Pending;
        public string Filename { get; set; }

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

        public ImageProcessing() { }

        public void Initialize(StorageFile outputFile)
        {
            this.outputFile = outputFile;
            IsInitialized = true;
            CurrentState = ProcessingState.Pending;
        }

        public async Task ProcessImageAsync(CancellationToken cancellationToken, int maxDegreeOfParallelism)
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

                CalculatePixels(softwareBitmap, cancellationToken, maxDegreeOfParallelism);               
            }
        }

        private unsafe  void CalculatePixels(SoftwareBitmap softwareBitmap, CancellationToken cancellationToken, int maxDegreeOfParallelism)
        {
            try
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

                        //GRAYSCALE
                        for (int i = 0; i < bufferLayout.Height; i++)
                        {
                            if(i % 100 == 0)
                                ProgressChanged?.Invoke((double)i / bufferLayout.Height / 2.0, ProcessingState.Processing);

                            if (cancellationToken.IsCancellationRequested)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                            }
                            if (CurrentState == ProcessingState.Pausing)
                            {
                                CurrentState = ProcessingState.Paused;
                                ProgressChanged?.Invoke((double)i / bufferLayout.Height / 2.0, CurrentState);
                                
                                pauseSemaphore.Wait();
                                pauseSemaphore.Release();
                                CurrentState = ProcessingState.Processing;
                            }
                            Parallel.For(0, bufferLayout.Width, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, j =>
                            {
                                byte value = (byte)((
                                        dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 0] +
                                        dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 1] +
                                        dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 2])
                                        / 3.0);
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 0] = value;
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 1] = value;
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 2] = value;
                            });

                        }

                        //FLOYD-STEINBERG DITHERING

                        for (int y = 0; y < bufferLayout.Height - 1; y++)
                        {
                            if(y%100 == 0)
                                ProgressChanged?.Invoke(0.5 + ((double)y / bufferLayout.Height), ProcessingState.Processing);

                            if (cancellationToken.IsCancellationRequested)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                            }
                            if (CurrentState == ProcessingState.Pausing)
                            {
                                CurrentState = ProcessingState.Paused;
                                ProgressChanged?.Invoke(0.5 + ((double)y / bufferLayout.Height), CurrentState);
                                
                                pauseSemaphore.Wait();
                                pauseSemaphore.Release();
                                CurrentState = ProcessingState.Processing;
                            }
                            for (int x = 1; x < bufferLayout.Width - 1; x++)
                            {

                                byte oldR = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * y + 4 * x + 2];
                                byte oldG = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * y + 4 * x + 1];
                                byte oldB = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * y + 4 * x + 0];

                                // ROUND COLOR WITH FACTOR 1
                                double newR = Math.Round((float)oldR / 255.0) * 255;
                                double newG = Math.Round((float)oldG / 255.0) * 255;
                                double newB = Math.Round((float)oldB / 255.0) * 255;

                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * y + 4 * x + 0] = (byte)newB;
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * y + 4 * x + 1] = (byte)newG;
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * y + 4 * x + 2] = (byte)newR;

                                int errR = (int)(oldR - newR);
                                int errG = (int)(oldG - newG);
                                int errB = (int)(oldB - newB);

                                //ADD PART OF ERROR TO SURROUNDING PIXELS
                                //X+1, Y * 7/16
                                int orgB1 = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * y + 4 * (x + 1) + 0];
                                int orgG1 = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * y + 4 * (x + 1) + 1];
                                int orgR1 = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * y + 4 * (x + 1) + 2];
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * y + 4 * (x + 1) + 0] = 
                                    (byte)Math.Max(0, Math.Min(255, orgB1 + (errB * 7 / 16.0)));
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * y + 4 * (x + 1) + 1] = 
                                    (byte)Math.Max(0, Math.Min(255, orgG1 + (errG * 7 / 16.0)));
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * y + 4 * (x + 1) + 2] = 
                                    (byte)Math.Max(0, Math.Min(255, orgR1 + (errR * 7 / 16.0)));

                                //X-1, Y+1 * 3/16
                                int orgB2 = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * (x - 1) + 0];
                                int orgG2 = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * (x - 1) + 1];
                                int orgR2 = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * (x - 1) + 2];
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * (x - 1) + 0] = 
                                    (byte)Math.Max(0, Math.Min(255, orgB2 + (errB * 3 / 16.0)));
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * (x - 1) + 1] = 
                                    (byte)Math.Max(0, Math.Min(255, orgG2 + (errG * 3 / 16.0)));
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * (x - 1) + 2] = 
                                    (byte)Math.Max(0, Math.Min(255, orgR2 + (errR * 3 / 16.0)));

                                //X, Y+1 * 5/16
                                int orgB3 = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * x + 0];
                                int orgG3 = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * x + 1];
                                int orgR3 = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * x + 2];
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * x + 0] = 
                                    (byte)Math.Max(0, Math.Min(255, orgB3 + (errB * 5 / 16.0)));
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * x + 1] = 
                                    (byte)Math.Max(0, Math.Min(255, orgG3 + (errG * 5 / 16.0)));
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * x + 2] = 
                                    (byte)Math.Max(0, Math.Min(255, orgR3 + (errR * 5 / 16.0)));

                                //X+1, Y+1 * 1/16
                                int orgB4 = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * (x + 1) + 0];
                                int orgG4 = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * (x + 1) + 1];
                                int orgR4 = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * (x + 1) + 2];
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * (x + 1) + 0] = 
                                    (byte)Math.Max(0, Math.Min(255, (orgB4 + (errB * 1 / 16.0))));
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * (x + 1) + 1] = 
                                    (byte)Math.Max(0, Math.Min(255, (orgG4 + (errG * 1 / 16.0))));
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * (y + 1) + 4 * (x + 1) + 2] = 
                                    (byte)Math.Max(0, Math.Min(255, orgR4 + (errR * 1 / 16.0)));
                            }
                        }
                        CurrentState = ProcessingState.Done;
                        ProgressChanged?.Invoke(1.0, CurrentState);
                    }
                }

                SaveSoftwareBitmapToFile(softwareBitmap);
            }
            catch(OperationCanceledException)
            {
                CurrentState = ProcessingState.Cancelled;
                ProgressChanged?.Invoke(0.0, CurrentState);
            }
            catch
            {
                CurrentState = ProcessingState.Error;
                ProgressChanged?.Invoke(0.0, CurrentState);
            }
        }

    private async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap)
        {
            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Create an encoder with the desired format
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                // Set the software bitmap
                encoder.SetSoftwareBitmap(softwareBitmap);
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

        private async Task WaitForTaskToComplete()
        {
            try
            {
                await processTask;
            }
            catch(OperationCanceledException)
            {
                CurrentState = ProcessingState.Cancelled;
                ProgressChanged?.Invoke(0.0, CurrentState);
            }
            finally
            {
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
                processTask = null;
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
                    processTask = Task.Factory.StartNew(async () => await ProcessImageAsync(cancellationTokenSource.Token, ImageProcessingManager.MaxDegreeOfParallelism), cancellationTokenSource.Token);
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

        public (string sourcePath, string outputPath) GetParameters()
        {
            if(outputFile != null)
                return (sourceFile.Path, outputFile.Path);
            return (sourceFile.Path, "");
        }
    }
}
