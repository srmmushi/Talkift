using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Talkift.Client.Services;

namespace Talkift.Client
{
    public static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [STAThread]
        static void Main(string[] args)
        {
            bool debugMode = false;
            foreach (var arg in args)
            {
                if (arg.Equals("--debug", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-d", StringComparison.OrdinalIgnoreCase))
                {
                    debugMode = true;
                    break;
                }
            }

            if (debugMode)
            {
                AllocConsole();
                Console.Title = "Talkift Debug Console";
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("===========================================");
                Console.WriteLine("  Talkift Debug Mode");
                Console.WriteLine("===========================================");
                Console.ResetColor();
            }

            CrashLogger.Initialize(debugMode);
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
