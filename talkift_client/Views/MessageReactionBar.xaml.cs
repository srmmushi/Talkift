using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Talkift.Client.Views
{
    public sealed partial class MessageReactionBar : UserControl
    {
        private readonly List<string> _quickReactions = new() { "\U0001F44D", "\U0001F44E", "\U00002764", "\U0001F602", "\U0001F622", "\U0001F44F" };
        private string _messageId = string.Empty;

        public event EventHandler<(string MessageId, string Emoji)>? ReactionAdded;

        public MessageReactionBar()
        {
            this.InitializeComponent();
        }

        public void Show(string messageId)
        {
            _messageId = messageId;
            ReactionContainer.Children.Clear();

            foreach (var emoji in _quickReactions)
            {
                var btn = new Button
                {
                    Content = emoji,
                    FontSize = 16,
                    Padding = new Thickness(4, 2, 4, 2),
                    CornerRadius = new CornerRadius(12),
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent),
                    BorderThickness = new Thickness(0),
                    Tag = emoji
                };
                btn.Click += ReactionButton_Click;
                ReactionContainer.Children.Add(btn);
            }

            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void ReactionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string emoji)
            {
                ReactionAdded?.Invoke(this, (_messageId, emoji));
                Hide();
            }
        }
    }
}
