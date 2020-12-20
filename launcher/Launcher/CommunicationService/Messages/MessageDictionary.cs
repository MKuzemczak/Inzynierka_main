using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Launcher.Exceptions;

namespace Launcher.CommunicationService.Messages
{
    public static class MessageDictionary
    {
        public static Dictionary<string, Type> MessageClassNameToType = new Dictionary<string, Type>()
        {
            { typeof(ExitRequest).Name, typeof(ExitRequest) },
            { typeof(FindBonesRequest).Name, typeof(FindBonesRequest) },
            { typeof(FindBonesRequestResult).Name, typeof(FindBonesRequestResult) },
            { typeof(SetupFinishedIndication).Name, typeof(SetupFinishedIndication) }
        };

        public static Type GetTypeFromClassName(string name)
        {
            if (!MessageClassNameToType.TryGetValue(name, out Type ret))
            {
                throw new MessageTypeNotFoundException(name);
            }

            return ret;
        }
    }
}
