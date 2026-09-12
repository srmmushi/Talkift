using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Talkift.Client.Views
{
    public sealed partial class MessageSearchPanel : UserControl
    {
        public event EventHandler<string>? SearchRequested;
        public event EventHandler? CloseRequested;

        public MessageSearchPanel()
        {
            this.InitializeComponent();
        }

        public void Show()
        {
            this.Visibility = Visibility.Visible;
            SearchInput.Focus(FocusState.Programmatic);
        }

        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
            SearchInput.Text = string.Empty;
            ResultCountText.Text = string.Empty;
        }

        public void SetResultCount(int count)
        {
            ResultCountText.Text = $"{count} found";
        }

        private void SearchInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var query = SearchInput.Text?.Trim();
                if (!string.IsNullOrEmpty(query))
                {
                    SearchRequested?.Invoke(this, query);
                }
            }
        }

        private void CloseSearchButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
