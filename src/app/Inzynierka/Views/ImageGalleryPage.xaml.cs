using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.Toolkit.Uwp.UI.Animations;

using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Inzynierka.Controls;
using Inzynierka.Models;
using Inzynierka.Helpers;
using Inzynierka.Services;
using Inzynierka.DatabaseAccess;
using System.Linq;
using Windows.UI.Xaml.Controls.Primitives;

namespace Inzynierka.Views
{
    public sealed partial class ImageGalleryPage : Page, INotifyPropertyChanged
    {
        public const string ImageGallerySelectedIdKey = "ImageGallerySelectedIdKey";

        #region PROPERTIES
        public ImageDataSource Source { get; set; }
        public FolderItemFilteredImagesProvider FilteredImagesProvider { get; set; }

        private bool IsItemClickedWithThisClick = false;
        private ImageItem ClickedItemItem { get; set; }
        private ImageItem RightTappedImageItem { get; set; }

        #endregion

        #region EVENTS

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler ImageClicked;

        public event EventHandler<ImageGalleryPageSelectionChangedEventArgs> SelectionChanged;

        #endregion

        public ImageGalleryPage()
        {
            InitializeComponent();
            Loaded += ImageGalleryPage_OnLoaded;
        }

        #region METHODS
        public void SetImagesProvider(FolderItemFilteredImagesProvider provider)
        {
            if (provider is null)
                return;

            if (FilteredImagesProvider != provider)
            {
                if (FilteredImagesProvider is object)
                {
                    FilteredImagesProvider.ContentsChanged -= SelectedContentFolder_ContentsChanged;
                }

                FilteredImagesProvider = provider;
                FilteredImagesProvider.ContentsChanged += SelectedContentFolder_ContentsChanged;
            }

            LoadNewData();
        }

        private void LoadNewData()
        {
            Source = ImageDataSource.GetDataSource(FilteredImagesProvider.GetRawImageItems());

            if (Source != null)
            {
                imagesGridView.ItemsSource = Source;
            }
        }

        // simple protection from multiple SetTagsToFilter called
        private int SetTagsToFilterRequestCntr = 0;

        public async Task SetTagsToFilterAsync(List<string> tags)
        {
            int cntrState = ++SetTagsToFilterRequestCntr;
            Source.StopTasks();

            // giving the data source time to cancel its work
            await Task.Delay(500);
            if (cntrState == SetTagsToFilterRequestCntr)
                FilteredImagesProvider?.FilterWithTags(tags);
        }

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        #endregion

        #region EVENT_HANDLERS
        private void ImageGalleryPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (FilteredImagesProvider?.Folder is object)
            {
                LoadNewData();
            }
        }

        private async void SelectedContentFolder_ContentsChanged(object sender, EventArgs e)
        {
            // This callback can occur on a different thread so we need to marshal it back to the UI thread
            await MainThreadDispatcherService.MarshalToMainThreadAsync(LoadNewData);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        #endregion

        #region GALLERY_EVENT_HANDLERS
        private void ImagesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ClickedItemItem = e.ClickedItem as ImageItem;
            IsItemClickedWithThisClick = true;
        }

        private void ImagesGridView_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (ClickedItemItem != null)
            {
                ImageNavigationHelper.ContainingDataSource = imagesGridView.ItemsSource as ImageDataSource;
                ImageNavigationHelper.ContainingFolder = FilteredImagesProvider.Folder;
                ImageNavigationHelper.SelectedImage = ClickedItemItem;
                ImageClicked?.Invoke(this, new EventArgs());
            }
        }

        private void ImagesGridView_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (!IsItemClickedWithThisClick)
            {
                imagesGridView.SelectedItems.Clear();
            }

            IsItemClickedWithThisClick = false;
        }

        private void ImagesGridView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            DragAndDropHelper.DraggedItems.Clear();
            DragAndDropHelper.DraggedItems.AddRange(imagesGridView.SelectedItems);
        }

        private void ThumbnailImage_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var selectedImages = imagesGridView.SelectedItems;
            var imageItem = grid.DataContext as ImageItem;

            RightTappedImageItem = imageItem;
            if (!selectedImages.Contains(imageItem))
            {
                selectedImages.Clear();
                selectedImages.Add(imageItem);
            }

            var menuFlyout = grid.ContextFlyout as MenuFlyout;
            menuFlyout.Items[2].Visibility = Visibility.Collapsed;
            if (selectedImages.Count > 1)
            {
                bool differentGroups = false;
                int prevGroupId = (selectedImages[0] as ImageItem).Group.Id;
                foreach (ImageItem item in selectedImages)
                {
                    if (item.Group.Id != prevGroupId)
                    {
                        differentGroups = true;
                        break;
                    }
                }
                if (differentGroups)
                {
                    menuFlyout.Items[2].Visibility = Visibility.Visible;
                }
            }
        }

        private void ImagesGridView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            if (DragAndDropHelper.DropSuccessful)
            {
                DragAndDropHelper.DropSuccessful = false;
            }

            DragAndDropHelper.DraggedItems.Clear();
        }

        private void ThumbnailGrid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Add to group";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsGlyphVisible = false;
        }

        private async void ThumbnailGrid_Drop(object sender, DragEventArgs e)
        {

        }

        private void ImagesGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, new ImageGalleryPageSelectionChangedEventArgs(imagesGridView.SelectedItems.Select(i => i as ImageItem).ToList()));
        }
        #endregion

        #region RIGHT_CLICK_EVENT_HANDLERS


        private async void CopyImage_Click(object sender, RoutedEventArgs e)
        {
            StorageFile file = ((sender as MenuFlyoutItem).DataContext as ImageItem).File;
            List<StorageFile> storageFiles = new List<StorageFile>(1);
            storageFiles.Add(file);
            var dataPackage = new DataPackage();
            dataPackage.SetStorageItems(storageFiles);
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            try
            {
                Clipboard.SetContent(dataPackage);
            }
            catch (Exception)
            {
                var messageDialog = new MessageDialog("It is filed to copy this file");
                await messageDialog.ShowAsync();
            }
        }
        private async void DeleteImageFromDisk_Click(object sender, RoutedEventArgs e)
        {
            var files = imagesGridView.SelectedItems;

            string title = "Delete image";
            string message = "If you delete this file, you won't be able to recover it. Do you want to proceed?";

            if (files.Count == 0)
                return;
            else if (files.Count > 1)
            {
                title = "Delete images";
                message = "If you delete those files, you won't be able to recover them. Do you want to proceed?";
            }

            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };
            ContentDialogResult result = await deleteFileDialog.ShowAsync();
            // Delete the file if the user clicked the primary button.
            /// Otherwise, do nothing.
            if (result == ContentDialogResult.Primary)
            {
                foreach (var file in files)
                {
                    if (file is ImageItem imageItem)
                    {
                        try
                        {
                            await imageItem.DeleteFromDiskAsync();
                            await DatabaseAccessService.DeleteImageAsync(imageItem.DatabaseId);
                        }
                        catch (Exception)
                        {
                            var messageDialog = new MessageDialog("Operation failed");
                            await messageDialog.ShowAsync();
                        }
                    }
                }
            }
        }

        private void RenameImage_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
            //TODO: change name of an image
        }

        #endregion

    }

    public class ImageGalleryPageSelectionChangedEventArgs : EventArgs
    {
        public List<ImageItem> SelectedItems { get; private set; }

        public ImageGalleryPageSelectionChangedEventArgs(List<ImageItem> selectedItems)
        {
            SelectedItems = selectedItems;
        }
    }
}
