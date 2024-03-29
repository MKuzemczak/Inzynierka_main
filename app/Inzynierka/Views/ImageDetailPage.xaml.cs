﻿using System;
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

using Inzynierka.Controls;
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
        private readonly ImageBoneDataManager xRayProcessor = new ImageBoneDataManager();

        private Point rectangleDrawStartingPoint { get; set; } = new Point();
        private Rectangle drawnRectangle { get; set; }
        private ImageBoneData CurrentImageBoneData { get; set; }


        public ImageDetailPage()
        {
            this.InitializeComponent();
            PrepareDrawnRect();
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
            await xRayProcessor.FindBonesAsync(new List<ImageItem>() { CurrentlyDisplayedImageItem }, FindBonesCallback);
        }

        private void clrButton_Click(object sender, RoutedEventArgs e)
        {
            innerGrid.Children.Clear();
        }

        private void PrepareDrawnRect()
        {
            drawnRectangle = new Rectangle()
            {
                Fill = new SolidColorBrush(Color.FromArgb(50, 0, 0, 255)),
                Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 0, 150)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                AllowDrop = true
            };

            var drawnRectFlyout = new Flyout();
            var verticalStack = new StackPanel() { Spacing = 10};
            var boneNameTextBox = new TextBox() { Width = 300 };
            var commentTextBox = new TextBox()
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Width = 300
            };
            var button = new Button() { Content = "Save", Visibility = Visibility.Collapsed };

            verticalStack.Children.Add(new TextBlock() { Text = "Bone name" });
            verticalStack.Children.Add(boneNameTextBox);
            verticalStack.Children.Add(new TextBlock() { Text = "Comment", Margin = new Thickness(0, 10, 0, 0) });
            verticalStack.Children.Add(commentTextBox);
            verticalStack.Children.Add(button);
            drawnRectFlyout.Content = verticalStack;
            drawnRectangle.ContextFlyout = drawnRectFlyout;

            TextChangedEventHandler textChangedHandler = (object o, TextChangedEventArgs ea) =>
            {
                if (boneNameTextBox.Text == "")
                {
                    button.Visibility = Visibility.Collapsed;
                    return;
                }
                button.Visibility = Visibility.Visible;
            };
            boneNameTextBox.TextChanged += textChangedHandler;
            commentTextBox.TextChanged += textChangedHandler;
            button.Click += async (object o, RoutedEventArgs ea) =>
            {
                if (CurrentImageBoneData is null)
                {
                    StateMessagingService.Instance.SendInfoMessage("Try finding bones first!", 5000);
                    return;
                }

                var boneData = new BoneData()
                {
                    X = (float)((drawnRectangle.Translation.X + displayedImage.ActualWidth / 2) / displayedImage.ActualWidth),
                    Y = (float)((drawnRectangle.Translation.Y + displayedImage.ActualHeight / 2) / displayedImage.ActualHeight),
                    W = (float)(drawnRectangle.ActualWidth / displayedImage.ActualWidth),
                    H = (float)(drawnRectangle.ActualHeight / displayedImage.ActualHeight),
                    Confidence = 1,
                    DetectedClassName = boneNameTextBox.Text,
                    Comment = commentTextBox.Text
                };
                CurrentImageBoneData.BoneSearchResults.Add(boneData);
                await xRayProcessor.SaveBoneDataAsync(CurrentlyDisplayedImageItem, CurrentImageBoneData);

                var grid = PrepareBoneHighlightRect(
                    CurrentlyDisplayedImageItem,
                    CurrentImageBoneData,
                    boneData,
                    drawnRectangle.Translation.X,
                    drawnRectangle.Translation.Y,
                    (float)drawnRectangle.ActualWidth,
                    (float)drawnRectangle.ActualHeight,
                    boneData.DetectedClassName); ;

                innerGrid.Children.Add(grid);
            };
            drawnRectFlyout.Closed += (object s, object e) =>
            {
                drawnRectangle.Width = 0;
                boneNameTextBox.Text = "";
                commentTextBox.Text = "";
            };
            drawnRectangle.DragOver += displayedImage_DragOver;
            drawnRectangle.Drop += displayedImage_Drop;

            innerGrid.Children.Add(drawnRectangle);
        }

        private void FindBonesCallback(List<ImageItem> requestedImageItems, List<ImageBoneData> results)
        {
            if (requestedImageItems[0] != CurrentlyDisplayedImageItem)
                return;

            innerGrid.Children.Clear();
            innerGrid.Children.Add(drawnRectangle);

            if (results.Count == 0 || results[0].BoneSearchResults is null)
                return;

            CurrentImageBoneData = results[0];

            foreach (var result in results[0].BoneSearchResults)
            {
                var grid = PrepareBoneHighlightRect(
                    requestedImageItems[0],
                    results[0],
                    result,
                    (float)((float)(result.X * displayedImage.ActualWidth) - (displayedImage.ActualWidth / 2)),
                    (float)((float)(result.Y * displayedImage.ActualHeight) - (displayedImage.ActualHeight / 2)),
                    (float)(result.W * displayedImage.ActualWidth),
                    (float)(result.H * displayedImage.ActualHeight),
                    result.DetectedClassName);

                innerGrid.Children.Add(grid);
            }
        }

        private Grid PrepareBoneHighlightRect(
            ImageItem imageItem,
            ImageBoneData imageBoneData,
            BoneData singleBoneData,
            float centerX,
            float centerY,
            float width,
            float height,
            string displayedText)
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
                Translation = new System.Numerics.Vector3(centerX, centerY, 0),
                Width = width,
                Height = height,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Fill = fillLight,
                //Stroke = strokeFocus,
                RadiusX = 20,
                RadiusY = 20,
                CanDrag = true,
                AllowDrop = true
            };


            ToolTipService.SetToolTip(rect, new ToolTip()
            {
                Content = displayedText
            });

            Grid.SetColumn(grid, 1);

            var boneClassNameTextBlock = new TextBlock()
            {
                Text = displayedText,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = strokeLight,
                CanDrag = true,
                AllowDrop = true
            };

            boneClassNameTextBlock.Translation = rect.Translation/* + new System.Numerics.Vector3(0, ((float)rect.Height / 2) - (float)text.FontSize, 0)*/;

            PointerEventHandler rectFocus = (object sender, PointerRoutedEventArgs e) =>
            {
                rect.Fill = fillFocus;
                //rect.Stroke = strokeFocus;
                boneClassNameTextBlock.Foreground = strokeFocus;
            };
            PointerEventHandler rectUnfocus = (object sender, PointerRoutedEventArgs e) =>
            {
                rect.Fill = fillLight;
                //rect.Stroke = strokeLight;
                boneClassNameTextBlock.Foreground = strokeLight;
            };

            rect.PointerEntered += rectFocus;
            rect.PointerExited += rectUnfocus;
            boneClassNameTextBlock.PointerEntered += rectFocus;
            boneClassNameTextBlock.PointerExited += rectUnfocus;

            var flyout = new Flyout();
            var flyoutGrid = new StackPanel() { Spacing = 10 };
            var flyoutTextBox = new TextBox()
            {
                Text = singleBoneData.Comment ?? "",
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Width = 300
            };
            var flyoutButton = new Button()
            {
                Content = "Save",
                HorizontalAlignment = HorizontalAlignment.Center,
                Visibility = Visibility.Collapsed
            };

            flyoutTextBox.TextChanged += (object o, TextChangedEventArgs ea) => { flyoutButton.Visibility = Visibility.Visible; };
            flyoutButton.Click += async (object sender, RoutedEventArgs ea) =>
            {
                singleBoneData.Comment = flyoutTextBox.Text;
                await xRayProcessor.SaveBoneDataAsync(imageItem, imageBoneData);
                flyoutButton.Visibility = Visibility.Collapsed;
            };

            flyoutGrid.Children.Add(new TextBlock()
            {
                Text = "Comment",
                Height = flyoutButton.Height,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                TextAlignment = TextAlignment.Center
            });
            flyoutGrid.Children.Add(flyoutTextBox);
            flyoutGrid.Children.Add(flyoutButton);
            flyout.Content = flyoutGrid;
            rect.ContextFlyout = flyout;

            TappedEventHandler tappedHandler = (object o, TappedRoutedEventArgs a) =>
            {
                rect.ContextFlyout.ShowAt(rect);
                rectFocus(null, null);
            };
            rect.Tapped += tappedHandler;
            boneClassNameTextBlock.Tapped += tappedHandler;

            grid.Children.Add(rect);
            grid.Children.Add(boneClassNameTextBlock);

            rect.DragStarting += displayedImage_DragStarting;
            rect.DragOver += displayedImage_DragOver;
            rect.PointerPressed += displayedImage_PointerPressed;
            rect.Drop += displayedImage_Drop;
            boneClassNameTextBlock.DragStarting += displayedImage_DragStarting;
            boneClassNameTextBlock.DragOver += displayedImage_DragOver;
            boneClassNameTextBlock.PointerPressed += displayedImage_PointerPressed;
            boneClassNameTextBlock.Drop += displayedImage_Drop;

            return grid;
        }

        private void displayedImage_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsContentVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;

            var point = e.GetPosition(displayedImage);

            drawnRectangle.Width = Math.Abs(point.X - rectangleDrawStartingPoint.X);
            drawnRectangle.Height = Math.Abs(point.Y - rectangleDrawStartingPoint.Y);
            drawnRectangle.Translation = new System.Numerics.Vector3(
                (float)((point.X - rectangleDrawStartingPoint.X) / 2 + rectangleDrawStartingPoint.X - displayedImage.ActualWidth / 2),
                (float)((point.Y - rectangleDrawStartingPoint.Y) / 2 + rectangleDrawStartingPoint.Y - displayedImage.ActualHeight / 2),
                0);
        }

        private void displayedImage_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            args.AllowedOperations = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
        }

        private void displayedImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var intermediate = e.GetIntermediatePoints(displayedImage);
            var pointerPoint = intermediate.Count == 0 ? e.GetCurrentPoint(displayedImage) : intermediate[0];
            rectangleDrawStartingPoint = pointerPoint.Position;
        }

        private void displayedImage_Drop(object sender, DragEventArgs e)
        {
            drawnRectangle.ContextFlyout.ShowAt(drawnRectangle);
        }
    }
}
