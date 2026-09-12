using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Talkift.Client.Services
{
    public static class CrashLogger
    {
        private static string LogDir => StorageService.LogsDir;

        private static string GetLogFilePath()
        {
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            return Path.Combine(LogDir, $"crash_{date}.log");
        }

        public static void Initialize()
        {
            try
            {
                Directory.CreateDirectory(LogDir);
                AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
                TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedException;
            }
            catch { }
        }

        public static void SetLogDirectory(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (e.ExceptionObject is Exception ex)
                {
                    LogException("AppDomain.UnhandledException", ex);
                }
            }
            catch { }
        }

        private static void OnTaskSchedulerUnobservedException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                LogException("TaskScheduler.UnobservedTaskException", e.Exception);
            }
            catch { }
        }

        public static void LogException(string source, Exception ex)
        {
            try
            {
                Directory.CreateDirectory(LogDir);
                var logFile = GetLogFilePath();
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var separator = new string('=', 80);

                var log = $@"
{separator}
[{timestamp}] CRASH LOG - Source: {source}
{separator}
Exception Type: {ex.GetType().FullName}
Message: {ex.Message}
Stack Trace:
{ex.StackTrace}
";

                if (ex.InnerException != null)
                {
                    log += $@"
--- Inner Exception ---
Type: {ex.InnerException.GetType().FullName}
Message: {ex.InnerException.Message}
Stack Trace:
{ex.InnerException.StackTrace}
";
                }

                log += $"{separator}{Environment.NewLine}";
                File.AppendAllText(logFile, log);
                Debug.WriteLine($"[CrashLogger] Logged {source}: {ex.Message}");
            }
            catch { }
        }

        public static void LogMessage(string message)
        {
            try
            {
                Directory.CreateDirectory(LogDir);
                var logFile = GetLogFilePath();
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                File.AppendAllText(logFile, $"[{timestamp}] {message}{Environment.NewLine}");
            }
            catch { }
        }
    }
}
