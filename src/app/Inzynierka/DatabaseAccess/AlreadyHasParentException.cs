using System;
using System.Diagnostics;

namespace Inzynierka.DatabaseAccess
{
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class AlreadyHasParentException : Exception
    {
        public AlreadyHasParentException() { }
        public AlreadyHasParentException(string message) : base(message) { }
        public AlreadyHasParentException(string message, Exception inner) : base(message, inner) { }
        protected AlreadyHasParentException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
