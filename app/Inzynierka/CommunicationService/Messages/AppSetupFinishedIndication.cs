﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inzynierka.CommunicationService.Messages
{
    public class AppSetupFinishedIndication : BaseIndication
    {
        public override string ToJson()
        {
            return PrepareJson();
        }
    }
}