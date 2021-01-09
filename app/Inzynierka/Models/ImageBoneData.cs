using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Inzynierka.Models
{
    public class ImageBoneData
    {
        [JsonPropertyName("image_path")]
        public string ImagePath { get; set; }

        [JsonPropertyName("bone_search_results")]
        public List<BoneData> BoneSearchResults { get; set; }
    }

}
