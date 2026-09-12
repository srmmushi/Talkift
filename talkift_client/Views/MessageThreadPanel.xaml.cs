using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Talkift.Client.Views
{
    public sealed partial class MessageThreadPanel : UserControl
    {
        private readonly ObservableCollection<string> _messages = new();
        private string _parentMessageId = string.Empty;

        public event EventHandler<(string ParentId, string Reply)>? ReplySent;

        public MessageThreadPanel()
        {
            this.InitializeComponent();
            ThreadMessages.ItemsSource = _messages;
        }

        public void Show(string parentMessageId, System.Collections.Generic.List<string> replies)
        {
            _parentMessageId = parentMessageId;
            _messages.Clear();
            foreach (var r in replies)
                _messages.Add(r);
            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void ReplyInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrWhiteSpace(ReplyInput.Text))
            {
                _messages.Add(ReplyInput.Text);
                ReplySent?.Invoke(this, (_parentMessageId, ReplyInput.Text));
                ReplyInput.Text = string.Empty;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();
    }
}
