using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Inzynierka.CommunicationService.Messages
{
    public class FindBonesRequest : BaseMessage
    {
        public FindBonesRequest(string sender, string receiver, int requestId, List<string> imagePaths)
        {
            Sender = sender;
            Receiver = receiver;
            RequestId = requestId;
            Contents = imagePaths;
        }

        public override string ToJson()
        {
            return PrepareJson();
        }
    }
}
