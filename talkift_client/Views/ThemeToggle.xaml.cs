using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Services;

namespace Talkift.Client.Views
{
    public sealed partial class ThemeToggle : UserControl
    {
        private bool _isDark;

        public event EventHandler<bool>? ThemeChanged;

        public ThemeToggle()
        {
            this.InitializeComponent();
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            _isDark = !_isDark;
            ThemeIcon.Glyph = _isDark ? "\uE708" : "\uE793";
            ThemeChanged?.Invoke(this, _isDark);
        }

        public void SetTheme(bool isDark)
        {
            _isDark = isDark;
            ThemeIcon.Glyph = _isDark ? "\uE708" : "\uE793";
        }
    }
}
