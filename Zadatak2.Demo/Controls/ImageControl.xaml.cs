using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Zadatak2.Demo.Controls
{
    public sealed partial class ImageControl : UserControl
    {

        public delegate void ImageProcessingActionCompletedDelegate(ImageProcessing imageProcessing, object sender);

        public event ImageProcessingActionCompletedDelegate ImageProcessingCancelled;
        public event ImageProcessingActionCompletedDelegate ImageProcessingPaused;
        public event ImageProcessingActionCompletedDelegate ImageProcessingResumed;
        public event ImageProcessingActionCompletedDelegate ImageProcessingStarted;
        public event ImageProcessingActionCompletedDelegate ImageProcessingCompleted;
        public event ImageProcessingActionCompletedDelegate ImageProcessingRemoved;

        private ImageProcessing ImageProcessing;

        public ImageControl()
        {
            this.InitializeComponent();
        }

        public ImageControl(ImageProcessing imageProcessing)
        {
            this.InitializeComponent();
            this.ImageTitle.Text = imageProcessing.Filename;
            this.ImageProcessing = imageProcessing;
            imageProcessing.ProgressChanged += ImageProcessing_ProgressChanged;


        }

        private async void ImageProcessing_ProgressChanged(double progress, ImageProcessing.ProcessingState processingState)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (!double.IsNaN(progress))
                    ProcessingProgressBar.Value = progress;
                ProcessingInfoTextBlock.Text = processingState.ToString();

                UpdateControlVisibility();
            });
        }

        private void UpdateControlVisibility()
        {
            ProcessingProgressBar.Visibility = CancelButton.Visibility = PauseButton.Visibility = (!(ImageProcessing.IsFinished || ImageProcessing.IsPending)).ToVisibility();
            StartButton.Visibility = (ImageProcessing.IsPending || ImageProcessing.IsFinished).ToVisibility();
            CancelButton.IsEnabled = ImageProcessing.CurrentState != ImageProcessing.ProcessingState.Cancelling && ImageProcessing.CurrentState != ImageProcessing.ProcessingState.Cancelled;
            PauseButton.Visibility = (ImageProcessing.CurrentState != ImageProcessing.ProcessingState.Pausing && ImageProcessing.CurrentState != ImageProcessing.ProcessingState.Paused && !ImageProcessing.IsFinished).ToVisibility();
            ResumeButton.Visibility = (ImageProcessing.CurrentState == ImageProcessing.ProcessingState.Paused).ToVisibility();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => ImageProcessingCancelled?.Invoke(ImageProcessing, this);

        private void PauseButton_Click(object sender, RoutedEventArgs e) => ImageProcessingPaused?.Invoke(ImageProcessing, this);

        private void StartButton_Click(object sender, RoutedEventArgs e) => ImageProcessingStarted?.Invoke(ImageProcessing, this);

        private void ResumeButton_Click(object sender, RoutedEventArgs e) => ImageProcessingResumed?.Invoke(ImageProcessing, this);

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) => FlyoutBase.ShowAttachedFlyout(sender as Grid);

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e) => ImageProcessingRemoved?.Invoke(ImageProcessing, this);
    }
}
