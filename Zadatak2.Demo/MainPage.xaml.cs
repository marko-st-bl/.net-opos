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
using Windows.ApplicationModel.Background;

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
            foreach(var imageProcessing in imageProcessingManager.ImageProcessings)
            {
                AddItemToStackPanel(imageProcessing);
            }
            RegisterPendingTasksMonitor();
        }

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
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                    ImageProcessing imageProcessing = new ImageProcessing(file);
                    imageProcessingManager.AddImageProcessing(imageProcessing);
                    await AddItemToStackPanel(imageProcessing);
                }
            }
        }

        private async void ConvertAllButton_Click(object sender, RoutedEventArgs e)
        {
            if(imageProcessingManager.ImageProcessings.Count < 1)
            {
                ContentDialog noImagesDialog = new ContentDialog
                {
                    Title = "No images",
                    Content = "Add or Capture images and try again.",
                    CloseButtonText = "OK"
                };
                await noImagesDialog.ShowAsync();
                return;
            }
            FolderPicker folderPicker = new FolderPicker() { SuggestedStartLocation = PickerLocationId.PicturesLibrary };
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);

                await imageProcessingManager.InitializeImageProcessings(folder);
                await imageProcessingManager.RunImageProcessings();
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
                imageControl.ImageProcessingResumed += ImageProcessingProgressControl_Resumed;
                imageControl.ImageProcessingRemoved += ImageProcessingProgressControl_Removed;
                ImagesStackPanel.Children.Add(imageControl);
            });
        }

        private async Task RemoveImageProcessing(ImageProcessing imageProcessing, ImageControl imageControl)
        {
            if(imageProcessing.IsFinished)
                await imageProcessing.Cancel(true);
            imageProcessingManager.RemoveImageProcessing(imageProcessing);
            if (imageControl != null)
                ImagesStackPanel.Children.Remove(imageControl);
        }

        private async void ImageProcessingProgressControl_Paused(ImageProcessing imageProcessing, object sender) => await imageProcessing.Pause();

        private async void ImageProcessingProgressControl_Cancelled(ImageProcessing imageProcessing, object sender) => await imageProcessing.Cancel();

        private async void ImageProcessingProgressControl_Resumed(ImageProcessing imageProcessing, object sender) => await imageProcessing.Resume();

        private async void ImageProcessingProgressControl_Removed(ImageProcessing imageProcessing, object sender) => await RemoveImageProcessing(imageProcessing, sender as ImageControl);

        private async void ImageProcessingProgressControl_Started(ImageProcessing imageProcessing, object sender)
        {
            if (imageProcessing.IsFinished)
                await imageProcessing.Reset();
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

        private async void TakePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;

            StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(photo);

            if (photo == null)
            {
                // User cancelled photo capture
                return;
            }

            ImageProcessing imageProcessing = new ImageProcessing(photo);
            imageProcessingManager.AddImageProcessing(imageProcessing);
            await AddItemToStackPanel(imageProcessing);
        }

        public static void RegisterPendingTasksMonitor()
        {
            var taskRegistered = false;
            var exampleTaskName = "PendindgTaskMonitor";

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == exampleTaskName)
                {
                    taskRegistered = true;
                    break;
                }
            }

            if (!taskRegistered)
            {
                var builder = new BackgroundTaskBuilder();

                builder.Name = exampleTaskName;
                builder.TaskEntryPoint = "Zadatak2.NotificationBackgroundTask";
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.TimeZoneChange, false));
            }
        }
    }
}
