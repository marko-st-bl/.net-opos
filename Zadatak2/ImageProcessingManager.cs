using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace Zadatak2
{
    public class ImageProcessingManager
    {

        const int downloadsPerCore = 2;

        private static readonly int maxConcurrentImages = Environment.ProcessorCount * downloadsPerCore;
        private readonly List<ImageProcessing> imageProcessings;
        
        public IReadOnlyList<ImageProcessing> ImageProcessings => imageProcessings;

        private ImageProcessingManager(List<ImageProcessing> imageProcessings) => this.imageProcessings = imageProcessings;
        private ImageProcessingManager() : this(new List<ImageProcessing>()) { }

        private static async Task<StorageFile> GetSerializationFile() => await ApplicationData.Current.LocalFolder.CreateFileAsync("imageProcessings.xml", CreationCollisionOption.OpenIfExists);

        public async Task InitializeImageProcessings(StorageFolder folder)
        {
            List<ImageProcessing> unitializedProcessings = imageProcessings.Where(x => !x.IsInitialized).ToList();
            foreach(var imageProcessing in unitializedProcessings)
            {
                StorageFile outputFile = await folder.CreateFileAsync(imageProcessing.Filename, CreationCollisionOption.ReplaceExisting);
                imageProcessing.Initialize(outputFile);
            }
        }

        public async Task RunImageProcessings()
        {
            int currentActiveProcessings = imageProcessings.Count(x=> x.CurrentState == ImageProcessing.ProcessingState.Processing);
            if(currentActiveProcessings < maxConcurrentImages)
            {
                List<ImageProcessing> pendingImageProcessings = imageProcessings.Where(x => x.CurrentState == ImageProcessing.ProcessingState.Pending && x.IsInitialized).ToList();
                foreach(var processing in imageProcessings)
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

                List<ImageProcessing> imageProcessings; //= xml.Elements().Select(x => new ImageProcessing(new StorageFile(x.Attribute("path"))).ToList());

                return new ImageProcessingManager();
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

        public async static Task Save()
        {

        }

        public void RemoveDownload(ImageProcessing imageProcessing) => imageProcessings.Remove(imageProcessing);
    }
}
