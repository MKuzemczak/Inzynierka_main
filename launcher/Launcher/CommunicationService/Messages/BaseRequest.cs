using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Launcher.CommunicationService.Messages
{
    public abstract class BaseRequest : BaseIndication
    {
        [JsonPropertyName("contents")]
        public virtual object Contents { get; set; }
    }
}
