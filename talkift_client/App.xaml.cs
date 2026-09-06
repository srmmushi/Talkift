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
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
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

            _window = new MainWindow();
            _window.Activate();
        }

        public static Window? CurrentWindow =>
            ((App)Current)._window;
    }
}
