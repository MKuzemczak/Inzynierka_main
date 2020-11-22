using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Inzynierka.Services
{
    public static class MainThreadDispatcherService
    {
        public static  CoreDispatcher MainThreadDispatcher;
        public static bool Initialized { get; private set; } = false;

        public static void Initialize(CoreDispatcher mainThreadDispatcher)
        {
            if (mainThreadDispatcher is null)
            {
                throw new ArgumentNullException("Argument " + nameof(mainThreadDispatcher) + " shouldn't be null");
            }

            MainThreadDispatcher = mainThreadDispatcher;
            Initialized = true;
        }

        public static async Task MarshalToMainThreadAsync(Action method)
        {
            if (!Initialized)
            {
                throw new Exceptions.MainThreadDispatcherServiceNotInitialized();
            }

            if (!MainThreadDispatcher.HasThreadAccess)
            {
                await MainThreadDispatcher.RunAsync(CoreDispatcherPriority.Normal, () => method());
            }
            else
            {
                method();
            }
        }

        public static async Task MarshalAsyncMethodToMainThreadAsync(Func<Task> method)
        {
            if (!Initialized)
            {
                throw new Exceptions.MainThreadDispatcherServiceNotInitialized();
            }

            if (!MainThreadDispatcher.HasThreadAccess)
            {
                await MainThreadDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await method());
            }
            else
            {
                await method();
            }
        }
    }
}
