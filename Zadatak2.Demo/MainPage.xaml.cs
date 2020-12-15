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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Zadatak2.Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        public ObservableCollection<StorageFile> Images { get; } = new ObservableCollection<StorageFile>();

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
                    ImageControl imageControl = new ImageControl(file);
                    Images.Add(file);
                    ImagesStackPanel.Children.Add(imageControl);
                    await Zadatak2.ImageProcessing.GrayscaleAsync(file);
                }
            }
            //Zadatak21.Processing.Grayscale("C:\\Users\\Marko\\Documents\\ViberDownloads\\beba.jpg");
        }
    }
}
