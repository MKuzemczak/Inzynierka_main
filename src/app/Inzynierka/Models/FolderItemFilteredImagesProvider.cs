using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Inzynierka.DatabaseAccess;
using Inzynierka.Services;
using Windows.Storage.Search;

namespace Inzynierka.Models
{
    public class FolderItemFilteredImagesProvider
    {
        private FolderItem _folder = null;
        public FolderItem Folder
        {
            get
            {
                return _folder;
            }
            set
            {
                if (value is null)
                    throw new ArgumentNullException("Tried to set Folder to null");

                if (_folder is object)
                    _folder.ContentsChanged -= Folder_ContentsChanged;
                _folder = value;
                _folder.ContentsChanged += Folder_ContentsChanged;
                Filter();
            }
        }

        public List<ImageItem> FilteredImages { get; set; } = new List<ImageItem>();
        protected List<string> TagsToFilter { get; set; } = new List<string>();

        public event EventHandler ContentsChanged;

        public FolderItemFilteredImagesProvider()
        {
        }

        public FolderItemFilteredImagesProvider(FolderItem folder) : this()
        {
            Folder = folder;
        }

        private async void Folder_ContentsChanged(object sender, EventArgs e)
        {
            await MainThreadDispatcherService.MarshalToMainThreadAsync(
                () =>
                {
                    Filter();
                    InvokeContentsChanged();
                });
        }

        protected void Filter()
        {
            FilteredImages.Clear();
            var allImages = Folder.GetRawImageItems();

            if (TagsToFilter is null || TagsToFilter.Count == 0)
            {
                FilteredImages.AddRange(allImages);
            }
            else
            {
                FilteredImages = allImages.
                    Where(i => TagsToFilter.Intersect(i.Tags).Count() == TagsToFilter.Count).ToList();
            }

            InvokeContentsChanged();
        }

        public void FilterWithTags(List<string> tags)
        {
            TagsToFilter = tags;
            Filter();
        }

        public List<string> GetTagsOfImages()
        {
            var hashSet = new HashSet<string>();
            Folder.GetRawImageItems().ForEach(i => i.Tags.ForEach(t => hashSet.Add(t)));

            return hashSet.ToList();
        }

        public List<ImageItem> GetRawImageItems()
        {
            var result = new List<ImageItem>();
            result.AddRange(FilteredImages);
            return result;
        }

        public void InvokeContentsChanged()
        {
            ContentsChanged?.Invoke(this, new EventArgs());
        }
    }
}
