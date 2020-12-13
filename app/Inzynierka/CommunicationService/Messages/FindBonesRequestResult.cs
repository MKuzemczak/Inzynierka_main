using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Inzynierka.CommunicationService.Messages
{
    public class BoneSearchResult
    {
        public float x { get; set; }
        public float y { get; set; }
        public float w { get; set; }
        public float h { get; set; }
        public float confidence { get; set; }
        public string detected_class_name { get; set; }
    }

    public class ImageBoneSearchResults
    {
        public string image_path { get; set; }
        public List<BoneSearchResult> bone_search_results { get; set; }
    }

    public class FindBonesRequestResult : BaseMessage
    {
        public List<ImageBoneSearchResults> ImagesBoneSearchResults { get; set; }

        public FindBonesRequestResult(string sender, string receiver, int requestId, List<ImageBoneSearchResults> imagesBoneSearchResults)
        {
            Sender = sender;
            Receiver = receiver;
            RequestId = requestId;
            ImagesBoneSearchResults = imagesBoneSearchResults;
        }

        public override string ToJson()
        {
            return PrepareJson(ImagesBoneSearchResults);
        }
    }


}
