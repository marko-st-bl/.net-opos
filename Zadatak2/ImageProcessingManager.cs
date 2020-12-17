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

        public static async Task<List<ImageProcessing>> Load()
        {
            List<ImageProcessing> imageProcessings = new List<ImageProcessing>();
            StorageFile file = await GetSerializationFile();

            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(List<ImageProcessing>));
                using (Stream stream = await file.OpenStreamForReadAsync())
                    imageProcessings = (List<ImageProcessing>)xml.Deserialize(stream);
            }
            catch
            {

            }
            return imageProcessings;
        }

        public void AddImageProcessing(ImageProcessing imageProcessing)
        {
            imageProcessings.Add(imageProcessing);
        }

        public async Task Save()
        {
            XmlSerializer xml = new XmlSerializer(typeof(List<ImageProcessing>));

            StorageFile file = await GetSerializationFile();

            using(Stream stream = await file.OpenStreamForWriteAsync())
            {
                xml.Serialize(stream, imageProcessings);
            }
        }

        public void RemoveImageProcessing(ImageProcessing imageProcessing) => imageProcessings.Remove(imageProcessing);
    }
}
