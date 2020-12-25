using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.Storage;

namespace Zadatak2
{
    public class ImageProcessingManager
    {

        const int processingsPerCore = 2;
        public static int MaxDegreeOfParallelism { get; set; } = 2;

        private static readonly int maxConcurrentImages = Environment.ProcessorCount * processingsPerCore;
        private readonly List<ImageProcessing> imageProcessings;
        
        public IReadOnlyList<ImageProcessing> ImageProcessings => imageProcessings;

        public ImageProcessingManager(List<ImageProcessing> imageProcessings) => this.imageProcessings = imageProcessings;
        public ImageProcessingManager() : this(new List<ImageProcessing>()) { }

        private static async Task<StorageFile> GetSerializationFile() => await ApplicationData.Current.LocalFolder.CreateFileAsync("imageProcessings.xml", CreationCollisionOption.OpenIfExists);

        public async Task InitializeImageProcessings(StorageFolder folder)
        {
            List<ImageProcessing> unitializedProcessings = imageProcessings.Where(x => !x.IsInitialized).ToList();
            foreach(var imageProcessing in unitializedProcessings)
            {
                StorageFile outputFile = await folder.CreateFileAsync(imageProcessing.Filename, CreationCollisionOption.GenerateUniqueName);
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(outputFile);
                imageProcessing.Initialize(outputFile);
            }
        }

        public async Task RunImageProcessings()
        {
            int currentActiveProcessings = imageProcessings.Count(x=> x.CurrentState == ImageProcessing.ProcessingState.Processing);
            if(currentActiveProcessings < maxConcurrentImages)
            {
                List<ImageProcessing> pendingImageProcessings = imageProcessings.Where(x => x.CurrentState == ImageProcessing.ProcessingState.Pending && x.IsInitialized).Take(maxConcurrentImages - currentActiveProcessings).ToList();
                foreach(var processing in pendingImageProcessings)
                {
                    await processing.Start();
                }
            }
        }

        public static async Task<ImageProcessingManager> Load()
        {
            try
            {
                StorageFile file = await GetSerializationFile();

                XElement xml;
                using (Stream stream = await file.OpenStreamForReadAsync())
                    xml = XElement.Load(stream);

                List<(string sourcePath, string outputPath)> attributes = xml.Elements().Select(x => (x.Attribute("sourcePath").Value, x.Attribute("outputPath").Value)).ToList();
                List<ImageProcessing> processings = new List<ImageProcessing>();
                foreach(var att in attributes)
                {
                    StorageFile sourceFile = await StorageFile.GetFileFromPathAsync(att.sourcePath);
                    if (att.outputPath != "")
                    {
                        StorageFile outputFile = await StorageFile.GetFileFromPathAsync(att.outputPath);
                        processings.Add(new ImageProcessing(sourceFile, outputFile));
                    }
                    else
                        processings.Add(new ImageProcessing(sourceFile));
                }
                return new ImageProcessingManager(processings);
            }
            catch
            {
                return new ImageProcessingManager();
            }
        }

        public void AddImageProcessing(ImageProcessing imageProcessing)
        {
            imageProcessings.Add(imageProcessing);
        }

        public async Task Save()
        {
            XElement xml = new XElement(nameof(ImageProcessingManager), imageProcessings.Select(x => x.GetParameters()).Select(
                x => new XElement(nameof(ImageProcessing), new XAttribute("sourcePath", x.sourcePath), new XAttribute("outputPath", x.outputPath))));

            StorageFile file = await GetSerializationFile();

            using (Stream stream = await file.OpenStreamForWriteAsync())
                xml.Save(stream);
        }

        public void RemoveImageProcessing(ImageProcessing imageProcessing) => imageProcessings.Remove(imageProcessing);
    }
}
