using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Inzynierka.Models
{
    public class BoneData
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

        [JsonPropertyName("comment")]
        public string Comment { get; set; }
    }
}
