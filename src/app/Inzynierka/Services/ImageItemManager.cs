using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Inzynierka.DatabaseAccess;
using Inzynierka.Models;
using Windows.Storage;

namespace Inzynierka.Services
{
    public static class ImageItemManager
    {
        private static Dictionary<int, ImageItem> AllImageItemsById = new Dictionary<int, ImageItem>();

        private static Dictionary<string, ImageItem> AllImageItemsByPath = new Dictionary<string, ImageItem>();

        private static void AddImageItemToDictionaries(ImageItem item)
        {
            AllImageItemsById.Add(item.DatabaseId, item);
            AllImageItemsByPath.Add(item.FilePath, item);
        }

        public static void DestroyImageItem(int databaseId, string path)
        {
            AllImageItemsById.Remove(databaseId);
            AllImageItemsByPath.Remove(path);
        }

        public static async Task DeleteImage(int databaseId, string path)
        {
            await DatabaseAccessService.DeleteImageAsync(databaseId);
            DestroyImageItem(databaseId, path);
        }

        private static ImageItem FindExistingInstanceFromPath(string path)
        {
            AllImageItemsByPath.TryGetValue(path, out ImageItem item);
            return item;
        }

        private static ImageItem FindExistingInstanceFromId(int id)
        {
            AllImageItemsById.TryGetValue(id, out ImageItem item);
            return item;
        }

        public static async Task<ImageItem> GetImageItemAsync(string path, ImageItem.Options storedImageDataMode = ImageItem.Options.None)
        {
            ImageItem result = FindExistingInstanceFromPath(path);

            if (result is object)
            {
                await result.SetStoredImageDataMode(storedImageDataMode);
                return result;
            }

            var databaseImage = await DatabaseAccessService.GetImageFromPathAsync(path);

            if (databaseImage is null)
            {
                databaseImage = await DatabaseAccessService.InsertImageAsync(path, false);
            }

            result = await ImageItem.FromDatabaseImageAsync(databaseImage, storedImageDataMode: storedImageDataMode);

            AddImageItemToDictionaries(result);

            return result;
        }

        public static async Task<ImageItem> GetImageItemAsync(DatabaseImage databaseImage, ImageItem.Options storedImageDataMode = ImageItem.Options.None)
        {
            if (databaseImage is null)
            {
                throw new ArgumentNullException(nameof(databaseImage));
            }

            ImageItem result = FindExistingInstanceFromId(databaseImage.Id);

            if (result is object)
            {
                await result.SetStoredImageDataMode(storedImageDataMode);
                return result;
            }

            result = await ImageItem.FromDatabaseImageAsync(databaseImage, storedImageDataMode: storedImageDataMode);

            AddImageItemToDictionaries(result);

            return result;
        }

        public static async Task<List<ImageItem>> GetImageItemsInFolderAsync(FolderItem folder)
        {
            var queriedImages = await folder.ImageQuery.GetFilesAsync();

            // TODO: change this db method, remove groups etc.
            var raw = await DatabaseAccessService.GetVirtualfolderImagesWithGroupsAndTags(folder.DatabaseId);
            var rawPathDict = raw.ToDictionary(i => i.Path, i => i);
            var queriedImagesCount = queriedImages.Count;
            var newImagesPaths = new List<string>();

            // finding images not in database and images in database that don't exist
            for (int i = queriedImagesCount - 1; i >= 0; i--)
            {
                DatabaseImage img = null;
                var path = queriedImages[i].Path;
                if (rawPathDict.TryGetValue(path, out img))
                {
                    rawPathDict.Remove(path);
                }
                else
                {
                    newImagesPaths.Add(path);
                }
            }

            // deleting non-existent images from database and from raw
            foreach (var image in rawPathDict.Values)
            {
                await DeleteImage(image.Id, image.Path);
                raw.Remove(image);
            }

            // Removing non-existing ImageItems, preserving existing and adding new
            var newImages = new List<ImageItem>();

            foreach (var path in newImagesPaths)
            {
                newImages.Add(await GetImageItemAsync(path, ImageItem.Options.None));
            }
            foreach (var rawImage in raw)
            {
                newImages.Add(await GetImageItemAsync(rawImage, ImageItem.Options.None));
            }

            return newImages;
        }
    }
}
