using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Inzynierka.CommunicationService.Messages
{
    public enum ResultStatus
    {
        Failed,
        Success
    }

    public abstract class BaseResult : BaseMessage
    {
        [JsonPropertyName("status")]
        public ResultStatus Status { get; set; }
    }
}
