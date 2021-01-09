using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Inzynierka.Models;

namespace Inzynierka.CommunicationService.Messages
{
    public class FindBonesRequestResult : BaseResult
    {
        [JsonPropertyName("contents")]
        public new List<ImageBoneData> Contents { get; set; }

        public override string ToJson()
        {
            return PrepareJson(this);
        }
    }


}
