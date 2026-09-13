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

        private static bool _debugMode;

        [STAThread]
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.Equals("--debug", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-d", StringComparison.OrdinalIgnoreCase))
                {
                    _debugMode = true;
                    break;
                }
            }

            if (_debugMode)
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

            CrashLogger.Initialize(_debugMode);
            CrashLogger.LogMessage("Application starting...");

            try
            {
                WinRT.ComWrappersSupport.InitializeComWrappers();
                CrashLogger.LogMessage("ComWrappers initialized");

                Application.Start((p) =>
                {
                    CrashLogger.LogMessage("Application.Start callback invoked");
                    var context = new DispatcherQueueSynchronizationContext(
                        DispatcherQueue.GetForCurrentThread());
                    SynchronizationContext.SetSynchronizationContext(context);
                    new App();
                });
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("Program.Main", ex);
            }
            finally
            {
                if (_debugMode)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Application has exited. Press any key to close this window...");
                    Console.ResetColor();
                    try { Console.ReadKey(true); }
                    catch { }
                }
            }
        }
    }
}
