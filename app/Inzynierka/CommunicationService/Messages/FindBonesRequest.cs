using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Inzynierka.CommunicationService.Messages
{
    public class FindBonesRequest : BaseRequest
    {
        public FindBonesRequest(string sender, string receiver, int requestId, List<string> imagePaths)
        {
            Sender = sender;
            Receiver = receiver;
            MessageId = requestId;
            Contents = imagePaths;
        }

        public override string ToJson()
        {
            return PrepareJson(this);
        }
    }
}
