using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Launcher.CommunicationService.Messages;

namespace Launcher.CommunicationService
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public object Message { get; set; }

        public MessageReceivedEventArgs(object message)
        {
            Message = message;
        }
    }
}
