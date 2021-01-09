using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

using Microsoft.Toolkit.Uwp.UI.Media;

using Inzynierka.Models;
using Inzynierka.Services;
using Inzynierka.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Inzynierka.CommunicationService;
using Inzynierka.CommunicationService.Messages;
using Inzynierka.MainThreadDispatcher;
using Inzynierka.StateMessaging;

namespace Inzynierka.Views
{
    public sealed partial class ImageDetailPage : Page, INotifyPropertyChanged
    {
        private int CurrentIndexInImageData { get; set; } = -1;

        private ImageItem _currentlyDisplayedImageItem;
        private ImageItem CurrentlyDisplayedImageItem
        {
            get { return _currentlyDisplayedImageItem; }
            set { Set(ref _currentlyDisplayedImageItem, value); }
        }

        private FolderItem CurrentlyAccessedFolder { get; set; }
        private List<ImageItem> ImageData { get; set; } = new List<ImageItem>();
        private CancellationTokenSource FlipCancellationTokenSource;
        private CancellationToken FlipCancellationToken;
        private readonly XRayProcessor xRayProcessor = new XRayProcessor();

        public ImageDetailPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        public async Task ShowAsync()
        {
            this.Visibility = Visibility.Visible;
            CurrentlyAccessedFolder = ImageNavigationHelper.ContainingFolder;
            CurrentlyDisplayedImageItem = ImageNavigationHelper.SelectedImage;
            ImageData = CurrentlyAccessedFolder.GetRawImageItems();
            CurrentIndexInImageData = ImageData.IndexOf(CurrentlyDisplayedImageItem);
            UpdateArrowsVisibility();
            await CurrentlyDisplayedImageItem.ToImageAsync();
        }

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private async Task UpdateImageToIndex(int index, CancellationToken ct = new CancellationToken())
        {
            if (ct.IsCancellationRequested)
                return;
            var oldImage = CurrentlyDisplayedImageItem;
            CurrentlyDisplayedImageItem = ImageData[CurrentIndexInImageData];
            if (ct.IsCancellationRequested)
                return;
            await CurrentlyDisplayedImageItem?.ToImageAsync(ct);
            await oldImage?.ToThumbnailAsync();
        }

        private void UpdateArrowsVisibility()
        {
            // TODO: take care of previous and next arrows
            //if (CurrentIndexInImageData == 0)
            //{
            //    previousArrow.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    previousArrow.Visibility = Visibility.Visible;
            //}

            //if (CurrentIndexInImageData  == ImageData.Count - 1)
            //{
            //    nextArrow.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    nextArrow.Visibility = Visibility.Visible;
            //}
        }

        private async void Previous_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (CurrentIndexInImageData == 0)
            {
                return;
            }

            CurrentIndexInImageData--;
            FlipCancellationTokenSource?.Cancel();
            FlipCancellationTokenSource = new CancellationTokenSource();
            FlipCancellationToken = FlipCancellationTokenSource.Token;
            UpdateArrowsVisibility();
            try
            {
                await UpdateImageToIndex(CurrentIndexInImageData, FlipCancellationToken);
            }
            catch (TaskCanceledException)
            {

            }
}

        private async void Next_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (CurrentIndexInImageData == ImageData.Count - 1)
            {
                return;
            }

            CurrentIndexInImageData++;
            FlipCancellationTokenSource?.Cancel();
            FlipCancellationTokenSource = new CancellationTokenSource();
            FlipCancellationToken = FlipCancellationTokenSource.Token;
            UpdateArrowsVisibility();
            try
            {
                await UpdateImageToIndex(CurrentIndexInImageData, FlipCancellationToken);
            }
            catch (TaskCanceledException)
            {

            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async void tmpButton_Click(object sender, RoutedEventArgs e)
        {
            await xRayProcessor.FindBonesAsync(CurrentlyDisplayedImageItem, FindBonesCallback);
        }

        private void FindBonesCallback(ImageItem requestedImageItem, List<ImageBoneSearchResults> results)
        {
            if (requestedImageItem != CurrentlyDisplayedImageItem)
                return;

            innerGrid.Children.Clear();

            if (results.Count == 0 || results[0].BoneSearchResults is null)
                return;

            foreach (var result in results[0].BoneSearchResults)
            {
                var fillColor = Color.FromArgb(50, 255, 255, 255);
                var fillLight = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                //var fillLight = new RadialGradientBrush()
                //{
                //    RadiusX = 1,
                //    RadiusY = 1,
                //    GradientStops = new GradientStopCollection()
                //    {
                //        new GradientStop() {Color = Color.FromArgb(50, 255, 0, 0), Offset = 0 },
                //        new GradientStop() {Color = Color.FromArgb(0, 255, 0, 0), Offset = 0.5 }
                //    },
                //    AlphaMode = AlphaMode.Premultiplied
                //};
                var fillFocus = new RadialGradientBrush()
                {
                    RadiusX = 1,
                    RadiusY = 1,
                    GradientStops = new GradientStopCollection()
                    {
                        new GradientStop() {Color = fillColor, Offset = 0 },
                        new GradientStop() {Color = Color.FromArgb(0, 255, 0, 0), Offset = 0.5 }
                    },
                    AlphaMode = AlphaMode.Premultiplied
                };
                var strokeLight = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
                var strokeFocus = new SolidColorBrush(Color.FromArgb(200, 255, 255, 250));

                var grid = new Grid();
                var rect = new Rectangle()
                {
                    Translation = new System.Numerics.Vector3(
                        (float)((float)(result.X * displayedImage.ActualWidth) - (displayedImage.ActualWidth / 2)),
                        (float)((float)(result.Y * displayedImage.ActualHeight) - (displayedImage.ActualHeight / 2)),
                        0),
                    Width = result.W * displayedImage.ActualWidth,
                    Height = result.H * displayedImage.ActualHeight,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Fill = fillLight,
                    //Stroke = strokeFocus,
                    RadiusX = 20,
                    RadiusY = 20
                };


                ToolTipService.SetToolTip(rect, new ToolTip()
                {
                    Content = result.DetectedClassName
                });

                Grid.SetColumn(grid, 1);

                var text = new TextBlock()
                {
                    Text = result.DetectedClassName,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = strokeLight
                };

                text.Translation = rect.Translation/* + new System.Numerics.Vector3(0, ((float)rect.Height / 2) - (float)text.FontSize, 0)*/;

                PointerEventHandler pointerEnteredHandler = (object sender, PointerRoutedEventArgs e) =>
                {
                    rect.Fill = fillFocus;
                    //rect.Stroke = strokeFocus;
                    text.Foreground = strokeFocus;
                };
                PointerEventHandler pointerExitedHandler = (object sender, PointerRoutedEventArgs e) =>
                {
                    rect.Fill = fillLight;
                    //rect.Stroke = strokeLight;
                    text.Foreground = strokeLight;
                };

                rect.PointerEntered += pointerEnteredHandler;
                rect.PointerExited += pointerExitedHandler;
                text.PointerEntered += pointerEnteredHandler;
                text.PointerExited += pointerExitedHandler;

                grid.Children.Add(rect);
                grid.Children.Add(text);

                innerGrid.Children.Add(grid);
            }
        }

        private void clrButton_Click(object sender, RoutedEventArgs e)
        {
            innerGrid.Children.Clear();
        }
    }
}
