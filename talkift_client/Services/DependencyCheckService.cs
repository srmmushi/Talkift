using System;
using System.Collections.Generic;

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
            try
            {
                var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BEB-234C9426A510}");
                if (key != null)
                {
                    var val = key.GetValue("pv");
                    return val != null && val.ToString() != "0.0.0.0";
                }
            }
            catch { }

            try
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BEB-234C9426A510}");
                if (key != null)
                {
                    var val = key.GetValue("pv");
                    return val != null && val.ToString() != "0.0.0.0";
                }
            }
            catch { }

            return false;
        }

        private static bool CheckVcRedist()
        {
            try
            {
                var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\X64");
                if (key != null)
                {
                    var val = key.GetValue("Installed");
                    return val != null && (int)val == 1;
                }
            }
            catch { }

            return false;
        }
    }
}
