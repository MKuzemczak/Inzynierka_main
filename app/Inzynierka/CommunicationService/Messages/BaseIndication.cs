using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inzynierka.CommunicationService.Messages
{
    public abstract class BaseIndication
    {
        [JsonPropertyName("sender")]
        public string Sender { get; set; }

        [JsonPropertyName("receiver")]
        public string Receiver { get; set; }

        [JsonPropertyName("message_id")]
        public int MessageId { get; set; }

        public abstract string ToJson();

        protected virtual string PrepareJson(object serializable)
        {
            return JsonSerializer.Serialize(serializable);
        }
    }
}
