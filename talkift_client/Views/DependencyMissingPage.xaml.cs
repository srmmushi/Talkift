using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Talkift.Client.Services;

namespace Talkift.Client.Views
{
    public sealed partial class DependencyMissingPage : Page
    {
        public DependencyMissingPage()
        {
            this.InitializeComponent();
            this.Loaded += DependencyMissingPage_Loaded;
        }

        private void DependencyMissingPage_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            try
            {
                TitleText.Text = LanguageService.GetString("DependenciesMissing");
                DescText.Text = LanguageService.GetString("MissingDependenciesDesc");
                RestartText.Text = LanguageService.GetString("RestartAfterInstall");
                SkipButton.Content = LanguageService.GetString("SkipAndContinue");
                CloseButton.Content = LanguageService.GetString("Close");
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("DependencyMissingPage.ApplyLocalization", ex);
            }
        }

        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is List<DependencyItem> missingDeps)
            {
                PopulateDependencies(missingDeps);
            }
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            app.LaunchMainWindow();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void PopulateDependencies(List<DependencyItem> missingDeps)
        {
            foreach (var dep in missingDeps)
            {
                var panel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 12,
                    Margin = new Thickness(0, 6, 0, 6)
                };

                var icon = new FontIcon
                {
                    Glyph = "\uE7BA",
                    FontSize = 18,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Orange)
                };
                panel.Children.Add(icon);

                var nameBlock = new TextBlock
                {
                    Text = dep.Name,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    FontSize = 14
                };
                panel.Children.Add(nameBlock);

                if (!string.IsNullOrEmpty(dep.DownloadUrl))
                {
                    var link = new HyperlinkButton
                    {
                        Content = "\u2193 " + LanguageService.GetString("Download"),
                        NavigateUri = new Uri(dep.DownloadUrl),
                        FontSize = 12
                    };
                    panel.Children.Add(link);
                }

                DependencyStackPanel.Children.Add(panel);
            }
        }
    }
}
