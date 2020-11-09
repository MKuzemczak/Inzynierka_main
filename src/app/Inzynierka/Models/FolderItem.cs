using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;

using Inzynierka.DatabaseAccess;
using System.Threading;
using System.IO;
using Windows.Storage.Search;

namespace Inzynierka.Models
{
    public class FolderItem
    {
        public static List<FolderItem> FolderItems { get; } = new List<FolderItem>();

        public const string NameInvalidCharacters = "\\/:*?\"<>|";

        public int DatabaseId;

        private List<FolderItem> _subfolders = new List<FolderItem>();
        public List<FolderItem> Subfolders
        {
            get
            {
                return _subfolders;
            }
            protected set
            {
                _subfolders = value;
            }
        }

        public FolderItem ParentFolder { get; protected set; }

        public string Name { get { return Folder.Name; } }
        public string Path { get { return Folder.Path; } }

        public StorageFolder Folder { get; protected set; }

        public List<string> TagsToFilter { get; protected set; } = new List<string>();

        public event EventHandler ContentsChanged;

        private List<ImageItem> AllImages { get; set; } = new List<ImageItem>();
        private List<ImageItem> FilteredImages { get; set; } = new List<ImageItem>();

        public StorageFileQueryResult ImageQuery { get; set; }


        protected static async Task<FolderItem> getNew(StorageFolder storageFolder)
        {
            var virtualFolder = await DatabaseAccessService.InsertVirtualFolderIfNotExistsAsync(storageFolder.Path);

            FolderItem result = new FolderItem
            {
                DatabaseId = virtualFolder.Id,
                Folder = storageFolder
            };

            List<string> fileTypeFilter = new List<string>();
            fileTypeFilter.Add(".jpg");
            fileTypeFilter.Add(".png");
            fileTypeFilter.Add(".gif");
            fileTypeFilter.Add(".bmp");
            var options = new QueryOptions(CommonFileQuery.OrderByName, fileTypeFilter)
            {
                FolderDepth = FolderDepth.Shallow
            };

            result.ImageQuery = result.Folder.CreateFileQueryWithOptions(options);
            result.ImageQuery.ContentsChanged += result.HandleQueryContentsChanged;

            await result.UpdateSubfoldersAsync();
            await result.UpdateQueryAsync();

            FolderItems.Add(result);

            return result;
        }

        protected static FolderItem findExistingInstance(string path)
        {
            foreach (var folder in FolderItems)
            {
                var path1 = System.IO.Path.GetFullPath(path);
                var path2 = System.IO.Path.GetFullPath(folder.Path);

                if (string.Equals(path1, path2))
                {
                    return folder;
                }
            }

            return null;
        }

        public static async Task<FolderItem> GetInstanceFromStorageFolder(StorageFolder storageFolder)
        {
            var result = findExistingInstance(storageFolder.Path);

            if (result != null)
            {
                return result;
            }

            return await getNew(storageFolder);
        }

        public async Task<IReadOnlyList<StorageFile>> GetStorageFilesRangeAsync(int firstIndex, int length)
        {
            var result = new List<StorageFile>();

            var range = FilteredImages.GetRange(firstIndex, length);

            foreach (var item in range)
            {
                try
                {
                    result.Add(await StorageFile.GetFileFromPathAsync(item.FilePath));
                }
                catch (FileNotFoundException)
                {
                    continue;
                }
            }

            return result;
        }

        public List<ImageItem> GetRawImageItems()
        {
            var result = new List<ImageItem>();
            result.AddRange(FilteredImages);
            return result;
        }

        public int GetFilesCount()
        {
            return FilteredImages.Count;
        }

        protected async Task UpdateSubfoldersAsync()
        {
            var storageFolders = await Folder.GetFoldersAsync();

            Subfolders.Clear();

            foreach (var item in storageFolders)
            {
                Subfolders.Add(await GetInstanceFromStorageFolder(item));
            }
        }

        public async Task RenameAsync(string newName)
        {
            if (newName.IndexOfAny(NameInvalidCharacters.ToCharArray()) != -1)
            {
                throw new FormatException();
            }
            await DatabaseAccessService.RenameVirtualFolderAsync(DatabaseId, newName);
            await Folder.RenameAsync(newName);
        }

        public async Task SetParentAsync(FolderItem folder)
        {
            if (folder is FolderItem)
            {
                await DatabaseAccessService.SetParentOfFolderAsync(DatabaseId, (folder as FolderItem).DatabaseId);
                ParentFolder?.Subfolders?.Remove(this);
                ParentFolder = folder;
                folder.Subfolders.Add(this);
            }
        }

        public async Task DeleteAsync()
        {
            if (Subfolders is object)
            {
                int subCount = Subfolders.Count;
                for (int i = subCount - 1; i >= 0; i--)
                {
                    await Subfolders[i].DeleteAsync();
                }
            }
            await DatabaseAccessService.DeleteVirtualFolderAsync(DatabaseId);
            ParentFolder?.Subfolders?.Remove(this);
            DatabaseId = -1;
            ParentFolder = null;
            Subfolders = null;
            Folder = null;
        }

        public async Task UpdateQueryAsync()
        {
            var raw = await DatabaseAccessService.GetVirtualfolderImagesWithGroupsAndTags(DatabaseId);
            var currentIds = AllImages.Select(i => i.DatabaseId).ToList();
            var rawCount = raw.Count;
            for (int i = rawCount - 1; i >= 0; i--)
            {
                for (int j = currentIds.Count - 1; j >= 0; j--)
                {
                    if (raw[i].Id == currentIds[j])
                    {
                        raw.RemoveAt(i);
                        currentIds.RemoveAt(j);
                        break;
                    }
                }
            }

            AllImages.RemoveAll(i => currentIds.Contains(i.DatabaseId));

            foreach (var item in raw)
            {
                AllImages.Add(await ImageItem.FromDatabaseImage(item, viewMode: ImageItem.Options.None));
            }

            ReorderImages();
        }

        public void ReorderImages()
        {
            var tmp = AllImages.OrderByDescending(i => i.Group.Id).ToList();
            AllImages = tmp;
            FilteredImages.Clear();

            if (TagsToFilter is null || TagsToFilter.Count == 0)
            {
                FilteredImages.AddRange(AllImages);
            }
            else
            {
                FilteredImages = AllImages.
                Where(i => TagsToFilter.Intersect(i.Tags).Count() == TagsToFilter.Count).ToList();
            }

            int prevGroupId = -1;
            int nextGroupId = -1;
            for (int i = 0; i < FilteredImages.Count; i++)
            {
                if (FilteredImages[i].Group is null ||
                    FilteredImages[i].Group.Id < 0)
                {
                    FilteredImages[i].PositionInGroup = Helpers.GroupPosition.None;
                    prevGroupId = -1;
                    continue;
                }

                int currentGroupId = FilteredImages[i].Group.Id;

                if (i + 1 == FilteredImages.Count)
                    nextGroupId = -1;
                else
                    nextGroupId = FilteredImages[i + 1].Group.Id;

                if (prevGroupId != currentGroupId &&
                    nextGroupId != currentGroupId)
                    FilteredImages[i].PositionInGroup = Helpers.GroupPosition.Only;
                else if (prevGroupId != currentGroupId)
                    FilteredImages[i].PositionInGroup = Helpers.GroupPosition.Start;
                else if (nextGroupId != currentGroupId)
                    FilteredImages[i].PositionInGroup = Helpers.GroupPosition.End;
                else
                    FilteredImages[i].PositionInGroup = Helpers.GroupPosition.Middle;

                prevGroupId = currentGroupId;
            }

            ContentsChanged?.Invoke(this, new EventArgs());
        }

        public async Task SetTagsToFilter(List<string> tags)
        {
            TagsToFilter = tags;
            await UpdateQueryAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <returns>List of database IDs</returns>
        public async Task<List<ImageItem>> AddFilesToFolder(IReadOnlyList<StorageFile> files)
        {
            var ids = new List<int>();
            foreach (var file in files)
            {
                ids.Add(await DatabaseAccessService.InsertImageAsync(file.Path, false, DatabaseId));
            }
            await UpdateQueryAsync();
            var result = AllImages.Where(i => ids.Contains(i.DatabaseId)).ToList();
            ContentsChanged?.Invoke(this, new EventArgs());
            return result;
        }

        public async Task<List<string>> GetTagsOfImagesAsync()
        {
            return await DatabaseAccessService.GetVirtualfolderTags(DatabaseId);
        }

        public void InvokeContentsChanged()
        {
            ContentsChanged?.Invoke(this, new EventArgs());
        }

        public async Task GroupImagesAsync(List<ImageItem> imageItems)
        {
            var group = await DatabaseAccessService.InsertSimilarityGroup("noname");

            foreach (var item in imageItems)
            {
                await item.AddToGroupAsync(group);
            }
        }

        public async Task GroupImagesAsync(List<int> imageIds)
        {
            var group = await DatabaseAccessService.InsertSimilarityGroup("noname");

            foreach (var item in AllImages)
            {
                if (imageIds.Remove(item.DatabaseId))
                {
                    await item.AddToGroupAsync(group);
                }
            }
        }

        public List<ImageItem> GetGroupOfImageItems(int groupId)
        {
            return AllImages.Where(i => (i.Group is object && i.Group.Id == groupId)).ToList();
        }

        protected async void HandleQueryContentsChanged(IStorageQueryResultBase sender, object args)
        {
            await UpdateSubfoldersAsync();
        }

        public static bool operator ==(FolderItem f1, FolderItem f2)
        {
            if ((f1 is object && f2 is null) ||
                (f1 is null && f2 is object))
                return false;

            if (f1 is null && f2 is null)
                return true;

            return (f1.Name == f2.Name &&
                f1.DatabaseId == f2.DatabaseId);
        }

        public static bool operator !=(FolderItem f1, FolderItem f2)
        {
            if ((f1 is object && f2 is null) ||
                (f1 is null && f2 is object))
                return true;

            if (f1 is null && f2 is null)
                return false;

            return !(f1.Name == f2.Name &&
                f1.DatabaseId == f2.DatabaseId);
        }

        public override bool Equals(object obj)
        {
            return obj is FolderItem item &&
                   Name == item.Name &&
                   DatabaseId == item.DatabaseId;
        }

        public override int GetHashCode()
        {
            int hashCode = 1010871291;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + DatabaseId.GetHashCode();
            return hashCode;
        }
    }
}
