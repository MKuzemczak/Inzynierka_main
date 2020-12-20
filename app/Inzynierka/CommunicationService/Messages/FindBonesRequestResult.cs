using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Inzynierka.CommunicationService.Messages
{
    public class BoneSearchResult
    {
        [JsonPropertyName("x")]
        public float X { get; set; }

        [JsonPropertyName("y")]
        public float Y { get; set; }

        [JsonPropertyName("w")]
        public float W { get; set; }

        [JsonPropertyName("h")]
        public float H { get; set; }

        [JsonPropertyName("confidence")]
        public float Confidence { get; set; }

        [JsonPropertyName("detected_class_name")]
        public string DetectedClassName { get; set; }
    }

    public class ImageBoneSearchResults
    {
        [JsonPropertyName("image_path")]
        public string ImagePath { get; set; }

        [JsonPropertyName("bone_search_results")]
        public List<BoneSearchResult> BoneSearchResults { get; set; }
    }

    public class FindBonesRequestResult : BaseResult
    {
        [JsonPropertyName("contents")]
        public new List<ImageBoneSearchResults> Contents { get; set; }

        public override string ToJson()
        {
            return PrepareJson();
        }
    }


}
