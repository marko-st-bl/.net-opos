using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Zadatak2.Demo.Controls;
using Zadatak2;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Zadatak2.Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        readonly ImageProcessingManager imageProcessingManager;
        public MainPage()
        {
            this.InitializeComponent();
            imageProcessingManager = (Application.Current as App).ImageProcessingManager;
        }

        public ObservableCollection<ImageProcessing> Images { get; } = new ObservableCollection<ImageProcessing>();

        private async void AddPhotos_Clicked(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");

            var files = await picker.PickMultipleFilesAsync();

            if(files.Count > 0)
            {
                foreach(StorageFile file in files)
                {
                    ImageProcessing imageProcessing = new ImageProcessing(file);
                    Images.Add(imageProcessing);
                    imageProcessingManager.AddImageProcessing(imageProcessing);
                    await AddItemToStackPanel(imageProcessing);
                }
            }
        }

        private async void ConvertAllButton_Click(object sender, RoutedEventArgs e)
        {
            ConvertAllButton.IsEnabled = false;

            FolderPicker folderPicker = new FolderPicker() { SuggestedStartLocation = PickerLocationId.PicturesLibrary };
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);

                await imageProcessingManager.InitializeImageProcessings(folder);
                await imageProcessingManager.RunImageProcessings();
            }
            ConvertAllButton.IsEnabled = true;
        }

        private async Task InitializeStackPanel(IReadOnlyList<ImageProcessing> imageProcessings)
        {
            foreach(ImageProcessing ip in imageProcessings)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ImageControl imageControl = new ImageControl(ip);
                    imageControl.ImageProcessingCancelled += null;
                    imageControl.ImageProcessingStarted += ImageProcessingProgressControl_Started;
                });
            }
        }

        private async Task AddItemToStackPanel(ImageProcessing imageProcessing)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ImageControl imageControl = new ImageControl(imageProcessing);
                imageControl.ImageProcessingCancelled += ImageProcessingProgressControl_Cancelled;
                imageControl.ImageProcessingStarted += ImageProcessingProgressControl_Started;
                imageControl.ImageProcessingPaused += ImageProcessingProgressControl_Paused;
                ImagesStackPanel.Children.Add(imageControl);
            });
        }

        private async void ImageProcessingProgressControl_Paused(ImageProcessing imageProcessing, object sender) => imageProcessing.Pause();

        private async void ImageProcessingProgressControl_Cancelled(ImageProcessing imageProcessing, object sender) => imageProcessing.Cancel();

        private async void ImageProcessingProgressControl_Started(ImageProcessing imageProcessing, object sender)
        {
            if (!imageProcessing.IsInitialized)
            {
                FileSavePicker savePicker = new FileSavePicker()
                {
                    SuggestedStartLocation = PickerLocationId.Downloads,
                    SuggestedFileName = "image"
                };
                savePicker.FileTypeChoices.Add("Any file type", new List<string>() { ".jpg" });

                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                    imageProcessing.Initialize(file);
            }
            if (imageProcessing.IsInitialized)
                await imageProcessing.Start();
        }
    }
}
