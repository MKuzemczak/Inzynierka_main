using System;
using System.Diagnostics;

namespace Inzynierka.Services.Exceptions
{
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    class MainThreadDispatcherServiceNotInitialized : Exception
    {
        public MainThreadDispatcherServiceNotInitialized() { }
        public MainThreadDispatcherServiceNotInitialized(string message) : base(message) { }
        public MainThreadDispatcherServiceNotInitialized(string message, Exception inner) : base(message, inner) { }
        protected MainThreadDispatcherServiceNotInitialized(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
