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
        public string Name
        {
            get { return GetType().Name; }
        }

        public string Sender { get; set; }
        public string Receiver { get; set; }

        public int RequestId { get; set; }

        public abstract string ToJson();

        protected string PrepareJson(object contents)
        {
            var obj = new { name = Name, sender = Sender, receiver = Receiver, contents };

            return JsonSerializer.Serialize(obj);
        }
    }
}
