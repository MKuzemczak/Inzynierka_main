using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.CommunicationService.Messages
{
    public class SetupFinishedIndication : BaseIndication
    {
        public override string ToJson()
        {
            return PrepareJson();
        }
    }
}
