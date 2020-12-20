using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inzynierka.CommunicationService.Messages
{
    public abstract class BaseMessage
    {
        [JsonPropertyName("sender")]
        public string Sender { get; set; }

        [JsonPropertyName("receiver")]
        public string Receiver { get; set; }

        [JsonPropertyName("request_id")]
        public int RequestId { get; set; }

        [JsonPropertyName("contents")]
        public virtual object Contents { get; set; }

        public abstract string ToJson();

        protected virtual string PrepareJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
