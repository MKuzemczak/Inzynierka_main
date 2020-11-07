using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inzynierka.Models
{
    public class MostRecentStateMessageUpdatedEventArgs : EventArgs
    {
        public StateMessage Message { get; }

        public MostRecentStateMessageUpdatedEventArgs(StateMessage message)
        {
            Message = message;
        }
    }
}
