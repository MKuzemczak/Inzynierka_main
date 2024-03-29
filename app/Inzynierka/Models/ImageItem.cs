﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;

using Inzynierka.Helpers;
using Inzynierka.DatabaseAccess;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Inzynierka.Models
{

    public class ImageItem : INotifyPropertyChanged
    {
        public enum Options : int
        {
            None = 0,
            Image = 1,
            Thumbnail = 2
        }


        public string Filename { get; set; }
        public BitmapImage ImageData { get; set; }

        public static BitmapImage BrokenFileIcon = new BitmapImage(new Uri("ms-appx:///Assets/Icons/broken-image.png"));

        public int DatabaseId { get; set; }

        public string FilePath { get; set; }

        public DatabaseSimilaritygroup Group { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        // Needed for displaying single image in the flip view
        public StorageFile File { get; set; }

        public Options StoredImageDataMode { get; set; }

        private GroupPosition _positionInGroup = GroupPosition.None;
        public GroupPosition PositionInGroup
        {
            get { return _positionInGroup; }
            set { Set(ref _positionInGroup, value); }
        }

        private bool _scanned = false;
        public bool Scanned
        {
            get { return _scanned; }
            set { Set(ref _scanned, value); }
        }

        private int _quality;
        public int Quality
        {
            get { return _quality; }
            set { Set(ref _quality, value); }
        }

        private bool _fileNotFound = false;
        public bool FileNotFound
        {
            get { return _fileNotFound; }
            set { Set(ref _fileNotFound, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async Task SetStorageFileFromPathAsync(string path)
        {
            File = await StorageFile.GetFileFromPathAsync(path);
        }

        private void MarkAsFileNotFound()
        {
            FileNotFound = true;
            ImageData = BrokenFileIcon;
        }

        public async Task ToImageAsync(CancellationToken ct = new CancellationToken())
        {
            if (StoredImageDataMode == Options.Image)
                return;
            if (ImageData == null)
                ImageData = new BitmapImage();
            if (ct.IsCancellationRequested)
                return;
            if (File == null)
            {
                try
                {
                    await SetStorageFileFromPathAsync(FilePath);
                }
                catch (FileNotFoundException e)
                {
                    MarkAsFileNotFound();
                    return;
                }

            }
            if (ct.IsCancellationRequested)
                return;
            using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                await File.OpenAsync(FileAccessMode.Read))
            {
                await ImageData.SetSourceAsync(fileStream).AsTask(ct);
            }

            StoredImageDataMode = Options.Image;
        }

        public async Task ToThumbnailAsync(CancellationToken ct = new CancellationToken())
        {
            if (StoredImageDataMode == Options.Thumbnail)
                return;
            if (ImageData == null)
                ImageData = new BitmapImage();
            if (File == null)
            {
                try
                {
                    await SetStorageFileFromPathAsync(FilePath);
                }
                catch (FileNotFoundException e)
                {
                    MarkAsFileNotFound();
                    return;
                }
                
            }
            if (ct.IsCancellationRequested)
                return;
            StorageItemThumbnail thumb = await File.GetThumbnailAsync(ThumbnailMode.SingleItem);
            if (ct.IsCancellationRequested)
                return;
            if (ImageData is object)
            {
                await ImageData.SetSourceAsync(thumb);
                StoredImageDataMode = Options.Thumbnail;
            }
        }

        public void ClearImageData()
        {
            StoredImageDataMode = Options.None;
            ImageData = null;
        }

        public async Task SetStoredImageDataMode(Options storedImageDataMode, CancellationToken ct = new CancellationToken())
        {
            if (storedImageDataMode == StoredImageDataMode)
                return;

            switch (storedImageDataMode)
            {
                case Options.None:
                    ClearImageData();
                    break;
                case Options.Image:
                    await ToImageAsync(ct);
                    break;
                case Options.Thumbnail:
                    await ToThumbnailAsync(ct);
                    break;
            }
        }

        // Needed to ensure only one request is in progress at once
        private static SemaphoreSlim gettingFileProperties = new SemaphoreSlim(1);

        public static async Task<ImageItem> FromDatabaseImageAsync(
            DatabaseImage dbimage, CancellationToken ct = new CancellationToken(), Options storedImageDataMode = Options.Image)
        {
            ImageItem result = new ImageItem()
            {
                DatabaseId = dbimage.Id,
                FilePath = dbimage.Path,
                Filename = Path.GetFileName(dbimage.Path),
                Group = dbimage.Group,
                Tags = dbimage.Tags,
                StoredImageDataMode = storedImageDataMode,
                Scanned = dbimage.Scanned
            };
            ct.ThrowIfCancellationRequested();
            switch (storedImageDataMode)
            {
                case Options.Image:
                    await result.ToImageAsync(ct);
                    break;
                case Options.Thumbnail:
                    await result.ToThumbnailAsync(ct);
                    break;
                default:
                    break;
            }

            return result;
        }


        // Fetches all the data for the specified file
        public async static Task<ImageItem> FromStorageFileAsync(
            StorageFile f, CancellationToken ct, Options options = Options.Image)
        {
            ImageItem item = new ImageItem()
            {
                Filename = f.Name,
                File = f,
                StoredImageDataMode = options
            };

            ct.ThrowIfCancellationRequested();

            BitmapImage img = new BitmapImage();

            if (options == Options.Image)
            {
                using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                await f.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    img.SetSource(fileStream);
                }
            }
            else if (options == Options.Thumbnail)
            {
                StorageItemThumbnail thumb = await f.GetThumbnailAsync(ThumbnailMode.SingleItem).AsTask(ct);
                ct.ThrowIfCancellationRequested();
                await img.SetSourceAsync(thumb).AsTask(ct);
            }

            item.ImageData = img;
            return item;
        }

        public async static Task<ImageItem> FromStorageFile(StorageFile f, Options options = Options.Image)
        {
            return await FromStorageFileAsync(f, new CancellationToken(), options);
        }

        public async Task DeleteFromDiskAsync()
        {
            await File?.DeleteAsync();
        }

        public async Task MarkScannedAsync()
        {
            Scanned = true;
            await DatabaseAccessService.SetImageScanned(DatabaseId, true);
        }

        public async Task AddToGroupAsync(DatabaseSimilaritygroup group)
        {
            Group = group;
            await DatabaseAccessService.UpdateImageToGroup(DatabaseId, group);
        }

        public async Task AddTagsAsync(List<string> tags)
        {
            if (File == null)
            {
                await SetStorageFileFromPathAsync(FilePath);
            }

            var props = await File.Properties.GetImagePropertiesAsync();
            foreach (var item in tags)
            {
                if (Tags.Contains(item))
                    continue;
                props.Keywords.Add(item);
                await DatabaseAccessService.InsertImageTagAsync(DatabaseId.ToString(), item);
                Tags.Add(item);
            }
            try
            {
                await props.SavePropertiesAsync();
            }
            catch (ArgumentException)
            {
                
            }
        }

        public async Task DeleteTagAsync(string tag)
        {
            if (File == null)
            {
                await SetStorageFileFromPathAsync(FilePath);
            }

            var props = await File.Properties.GetImagePropertiesAsync();
            if (props.Keywords.Remove(tag))
                await props.SavePropertiesAsync();

            await DatabaseAccessService.DeleteImageTagAsync(DatabaseId, tag);
            Tags.Remove(tag);
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

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
