using System;
using System.Diagnostics;

namespace Inzynierka.Exceptions
{
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    class MainThreadDispatcherServiceNotInitializedException : Exception
    {
        public MainThreadDispatcherServiceNotInitializedException() { }
        public MainThreadDispatcherServiceNotInitializedException(string message) : base(message) { }
        public MainThreadDispatcherServiceNotInitializedException(string message, Exception inner) : base(message, inner) { }
        protected MainThreadDispatcherServiceNotInitializedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
