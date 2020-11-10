using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inzynierka.Models
{
    public class FilteredItemsProvider : FolderItem
    {
        private List<ImageItem> FilteredImages { get; set; } = new List<ImageItem>();
        public List<string> TagsToFilter { get; protected set; } = new List<string>();


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

            ContentsChanged?.Invoke(this, new EventArgs());
        }


        public async Task FilterWithTags(List<string> tags)
        {
            TagsToFilter = tags;
            await UpdateQueryAsync();
        }
    }
}
