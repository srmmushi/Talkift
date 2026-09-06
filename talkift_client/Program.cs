using Microsoft.UI.Xaml;
using System;
using System.Threading;
using Talkift.Client.Services;

namespace Talkift.Client
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            CrashLogger.Initialize();
            CrashLogger.LogMessage("Application starting...");

            WinRT.ComWrappersSupport.InitializeComWrappers();
            Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(
                    DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                new App();
            });
        }
    }
}
