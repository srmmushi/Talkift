using System;
using System.Collections.Generic;
using System.IO;

namespace Talkift.Client.Views
{
    public class DependencyItem
    {
        public string Name { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public bool IsInstalled { get; set; }
    }

    public static class DependencyCheckService
    {
        public static List<DependencyItem> CheckAll()
        {
            var result = new List<DependencyItem>();

            result.Add(new DependencyItem
            {
                Name = "Microsoft Edge WebView2 Runtime",
                DownloadUrl = "https://go.microsoft.com/fwlink/p/?LinkId=2124703",
                IsInstalled = CheckWebView2()
            });

            result.Add(new DependencyItem
            {
                Name = "Microsoft Visual C++ Redistributable (2015-2022)",
                DownloadUrl = "https://aka.ms/vs/17/release/vc_redist.x64.exe",
                IsInstalled = CheckVcRedist()
            });

            return result;
        }

        private static bool CheckWebView2()
        {
            // Check 1: WOW6432Node path (HKLM)
            try
            {
                var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BEB-234C9426A510}");
                if (key != null)
                {
                    var val = key.GetValue("pv");
                    if (val != null && val.ToString() != "0.0.0.0")
                        return true;
                }
            }
            catch { }

            // Check 2: Non-WOW64 path (HKLM)
            try
            {
                var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BEB-234C9426A510}");
                if (key != null)
                {
                    var val = key.GetValue("pv");
                    if (val != null && val.ToString() != "0.0.0.0")
                        return true;
                }
            }
            catch { }

            // Check 3: HKCU path
            try
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BEB-234C9426A510}");
                if (key != null)
                {
                    var val = key.GetValue("pv");
                    if (val != null && val.ToString() != "0.0.0.0")
                        return true;
                }
            }
            catch { }

            // Check 4: HKCU WOW6432Node path
            try
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BEB-234C9426A510}");
                if (key != null)
                {
                    var val = key.GetValue("pv");
                    if (val != null && val.ToString() != "0.0.0.0")
                        return true;
                }
            }
            catch { }

            // Check 5: Edge browser is installed (WebView2 is built-in since Edge 79)
            try
            {
                var edgePaths = new[]
                {
                    @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                    @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Edge\Application\msedge.exe")
                };

                foreach (var path in edgePaths)
                {
                    if (File.Exists(path))
                        return true;
                }
            }
            catch { }

            // Check 6: WebView2Loader.dll exists in known locations
            try
            {
                var systemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
                if (File.Exists(Path.Combine(systemDir, "WebView2Loader.dll")))
                    return true;

                var sysWOW64 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64");
                if (File.Exists(Path.Combine(sysWOW64, "WebView2Loader.dll")))
                    return true;
            }
            catch { }

            // Check 7: Registry key under EdgeUpdate\Wow6432Node
            try
            {
                using var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(
                    @"WebView2Loader.dll\CLSID");
                if (key != null)
                    return true;
            }
            catch { }

            return false;
        }

        private static bool CheckVcRedist()
        {
            // Check 1: Correct registry path for VC++ Redistributable
            var registryPaths = new[]
            {
                @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64",
                @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\X64",
                @"SOFTWARE\WOW6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x64",
                @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\ARM64",
                @"SOFTWARE\WOW6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\ARM64"
            };

            foreach (var path in registryPaths)
            {
                try
                {
                    var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path);
                    if (key != null)
                    {
                        var installed = key.GetValue("Installed");
                        if (installed != null && (int)installed == 1)
                            return true;
                    }
                }
                catch { }
            }

            // Check 2: vcruntime140.dll exists in system directories
            try
            {
                var systemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
                if (File.Exists(Path.Combine(systemDir, "vcruntime140.dll")))
                    return true;

                var sysWOW64 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64");
                if (File.Exists(Path.Combine(sysWOW64, "vcruntime140.dll")))
                    return true;

                // Check also in native system dir
                if (Environment.Is64BitOperatingSystem)
                {
                    var sysNative = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysNative");
                    if (File.Exists(Path.Combine(sysNative, "vcruntime140.dll")))
                        return true;
                }
            }
            catch { }

            // Check 3: Check installed programs in registry
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using var subKey = key.OpenSubKey(subKeyName);
                        if (subKey != null)
                        {
                            var displayName = subKey.GetValue("DisplayName")?.ToString() ?? "";
                            if (displayName.Contains("Visual C++") && displayName.Contains("2015"))
                                return true;
                            if (displayName.Contains("Visual C++") && displayName.Contains("2022"))
                                return true;
                            if (displayName.Contains("Visual C++") && displayName.Contains("Redistributable"))
                                return true;
                        }
                    }
                }
            }
            catch { }

            // Check 4: Also check WOW6432Node for installed programs
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using var subKey = key.OpenSubKey(subKeyName);
                        if (subKey != null)
                        {
                            var displayName = subKey.GetValue("DisplayName")?.ToString() ?? "";
                            if (displayName.Contains("Visual C++") && displayName.Contains("Redistributable"))
                                return true;
                        }
                    }
                }
            }
            catch { }

            return false;
        }
    }
}
