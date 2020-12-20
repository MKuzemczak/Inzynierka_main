using System;
using System.Diagnostics;

namespace Inzynierka.Exceptions
{
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    class MessageTypeNotFoundException : Exception
    {
        public MessageTypeNotFoundException() { }
        public MessageTypeNotFoundException(string typeName) : base($"Exception: Type '{typeName}' not found") { }
        public MessageTypeNotFoundException(string typeName, Exception inner) : base($"Exception: Type '{typeName}' not found", inner) { }
        protected MessageTypeNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
