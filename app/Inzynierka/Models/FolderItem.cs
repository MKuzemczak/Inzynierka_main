using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Core;

using Inzynierka.DatabaseAccess;
using Inzynierka.Services;
using Inzynierka.Models.ModelsEventArgs;

namespace Inzynierka.Models
{
    public class FolderItem
    {
        public static Dictionary<string, FolderItem> FolderItems { get; } = new Dictionary<string, FolderItem>();

        public const string NameInvalidCharacters = "\\/:*?\"<>|";

        public int DatabaseId { get; set; } = -1;

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

        public bool IsRoot { get; set; } = false;

        public StorageFolder Folder { get; protected set; }

        public event EventHandler ContentsChanged;
        public event EventHandler<FolderItemRenamedEventArgs> Renamed;
        public event EventHandler RemovedFromApp;

        private Dictionary<int, ImageItem> _allImagesById = new Dictionary<int, ImageItem>();
        protected Dictionary<int, ImageItem> AllImagesById
        {
            get { return _allImagesById; }
            set
            {
                _allImagesById = value;
                AllImagesByPath = _allImagesById.ToDictionary(i => i.Value.FilePath, i => i.Value);
            }
        }

        protected IReadOnlyDictionary<string, ImageItem> AllImagesByPath { get; set; } = new Dictionary<string, ImageItem>();


        public StorageFileQueryResult ImageQuery { get; set; }

        protected static async Task<FolderItem> GetNew(StorageFolder storageFolder, DatabaseVirtualFolder virtualFolder = null)
        {
            FolderItem result = new FolderItem();
            result.Folder = storageFolder;

            if (virtualFolder is object)
            {
                result.DatabaseId = virtualFolder.Id;
                result.IsRoot = virtualFolder.IsRoot;
            }

            result.InitializeImageQuery();

            await result.UpdateSubfoldersAsync();
            await result.ReloadImagesAsync();

            FolderItems.Add(result.Path, result);

            return result;
        }

        public static async Task<FolderItem> GetInstanceFromStorageFolder(StorageFolder storageFolder, bool isRoot = false)
        {
            FolderItem result = null;

            if (FolderItems.TryGetValue(storageFolder.Path, out result))
            {
                return result;
            }

            var virtualFolder = await DatabaseAccessService.InsertVirtualFolderIfNotExistsAsync(storageFolder.Path, isRoot);

            return await GetNew(storageFolder, virtualFolder);
        }

        public static async Task<FolderItem> GetInstanceFromDatabaseVirtualFolder(DatabaseVirtualFolder virtualFolder)
        {
            FolderItem result = null;

            if (FolderItems.TryGetValue(virtualFolder.Path, out result))
            {
                return result;
            }

            var storageFolder = await StorageFolder.GetFolderFromPathAsync(virtualFolder.Path);

            return await GetNew(storageFolder, virtualFolder);
        }

        protected void InitializeImageQuery()
        {
            List<string> fileTypeFilter = new List<string>();
            fileTypeFilter.Add(".jpg");
            fileTypeFilter.Add(".png");
            fileTypeFilter.Add(".gif");
            fileTypeFilter.Add(".bmp");
            var options = new QueryOptions(CommonFileQuery.OrderByName, fileTypeFilter)
            {
                FolderDepth = FolderDepth.Shallow
            };

            ImageQuery = Folder.CreateFileQueryWithOptions(options);
            ImageQuery.ContentsChanged += HandleQueryContentsChanged;
        }


        public List<ImageItem> GetRawImageItems()
        {
            var result = new List<ImageItem>();
            result.AddRange(AllImagesById.Values);
            return result;
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
            var oldName = Folder.Name;
            await DatabaseAccessService.RenameVirtualFolderAsync(DatabaseId, newName);
            await Folder.RenameAsync(newName);
            InvokeRenamed(oldName, newName);
        }

        public async Task DeleteAsync()
        {
            // TODO: such delete should be available only for root folders. Consider moving all getInstance, FolderItems, delete functionality to folderManagerService
            FolderItems.Remove(Path);
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
            InvokeRemovedFromApp();
        }

        public virtual async Task ReloadImagesAsync()
        {
            var list = await ImageItemManager.GetImageItemsInFolderAsync(this);
            AllImagesById = list.ToDictionary(i => i.DatabaseId, i => i);
        }

        public async Task<FolderItem> CreateSubfolderAsync(string name)
        {
            var storageFolder = await Folder.CreateFolderAsync(name);
            return await GetInstanceFromStorageFolder(storageFolder);
        }

        public void InvokeContentsChanged()
        {
            ContentsChanged?.Invoke(this, new EventArgs());
        }

        public void InvokeRenamed(string oldName, string newName)
        {
            Renamed?.Invoke(this, new FolderItemRenamedEventArgs(oldName, newName));
        }

        public void InvokeRemovedFromApp()
        {
            RemovedFromApp?.Invoke(this, new EventArgs());
        }


        protected virtual async void HandleQueryContentsChanged(IStorageQueryResultBase sender, object args)
        {
            await MainThreadDispatcherService.MarshalAsyncMethodToMainThreadAsync(
                async () =>
                {
                    await UpdateSubfoldersAsync();
                    await ReloadImagesAsync();
                    InvokeContentsChanged();
                });
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
