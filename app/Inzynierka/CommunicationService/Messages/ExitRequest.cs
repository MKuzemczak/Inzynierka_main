using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inzynierka.CommunicationService.Messages
{
    public class ExitRequest : BaseMessage
    {
        public ExitRequest(string sender, string receiver, int requestId)
        {
            Sender = sender;
            Receiver = receiver;
            RequestId = requestId;
        }

        public override string ToJson()
        {
            return PrepareJson();
        }
    }
}
