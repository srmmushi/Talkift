using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Services;
using Talkift.Client.Views;

namespace Talkift.Client
{
    public partial class App : Application
    {
        private Window? _window;

        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            CrashLogger.LogException("Application.UnhandledException", e.Exception);
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                var storage = new StorageService();
                var savedPath = await storage.LoadAsync<string>("storage_path");
                if (!string.IsNullOrWhiteSpace(savedPath) && System.IO.Directory.Exists(savedPath))
                {
                    StorageService.SetDataDirectory(savedPath);
                }

                await LanguageService.LoadLanguageAsync();

                var missingDeps = DependencyCheckService.CheckAll();
                var reallyMissing = missingDeps.FindAll(d => !d.IsInstalled);

                if (reallyMissing.Count > 0)
                {
                    _window = new Window();
                    _window.Title = LanguageService.GetString("DependenciesMissing");
                    _window.Content = new Frame();
                    _window.Activate();

                    var frame = _window.Content as Frame;
                    if (frame != null)
                    {
                        frame.Navigate(typeof(DependencyMissingPage), reallyMissing);
                    }
                    return;
                }

                LaunchMainWindow();
            }
            catch (System.Exception ex)
            {
                CrashLogger.LogException("App.OnLaunched", ex);
                throw;
            }
        }

        public void LaunchMainWindow()
        {
            try
            {
                if (_window != null)
                {
                    _window.Close();
                }

                _window = new MainWindow();
                _window.Activate();
            }
            catch (System.Exception ex)
            {
                CrashLogger.LogException("App.LaunchMainWindow", ex);
                throw;
            }
        }

        public static Window? CurrentWindow =>
            ((App)Current)._window;
    }
}
