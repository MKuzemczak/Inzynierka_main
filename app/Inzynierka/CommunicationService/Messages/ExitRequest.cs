using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inzynierka.CommunicationService.Messages
{
    public class ExitRequest : BaseMessage
    {
        public ExitRequest(string sender, string receiver)
        {
            Sender = sender;
            Receiver = receiver;
        }

        public override string ToJson()
        {
            return PrepareJson();
        }
    }
}
