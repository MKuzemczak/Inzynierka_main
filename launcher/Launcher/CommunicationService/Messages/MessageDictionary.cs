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
            { typeof(AppSetupFinishedIndication).Name, typeof(AppSetupFinishedIndication) },
            { typeof(ExitIndication).Name, typeof(ExitIndication) },
            { typeof(FindBonesRequest).Name, typeof(FindBonesRequest) },
            { typeof(FindBonesRequestResult).Name, typeof(FindBonesRequestResult) },
            { typeof(PythonSetupFinishedIndication).Name, typeof(PythonSetupFinishedIndication) }
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
