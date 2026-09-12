using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Services;

namespace Talkift.Client.Views
{
    public sealed partial class ReplyBar : UserControl
    {
        private string _replyToMessageId = string.Empty;
        private string _replyToSender = string.Empty;

        public string ReplyToMessageId => _replyToMessageId;
        public string ReplyToSender => _replyToSender;

        public event EventHandler? CancelReply;

        public ReplyBar()
        {
            this.InitializeComponent();
        }

        public void ShowReply(string messageId, string senderName, string content)
        {
            _replyToMessageId = messageId;
            _replyToSender = senderName;
            ReplyToText.Text = LanguageService.GetString("ReplyTo") + " " + senderName;
            OriginalMessageText.Text = content;
            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
            _replyToMessageId = string.Empty;
            _replyToSender = string.Empty;
        }

        private void CancelReplyButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            CancelReply?.Invoke(this, EventArgs.Empty);
        }
    }
}
