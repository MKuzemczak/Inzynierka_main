using System;
using System.Diagnostics;

namespace Inzynierka.Views.Exceptions
{
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class NoParentFolderSelectedException : Exception
    {
        public NoParentFolderSelectedException() { }
        public NoParentFolderSelectedException(string message) : base(message) { }
        public NoParentFolderSelectedException(string message, Exception inner) : base(message, inner) { }
        protected NoParentFolderSelectedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
