using System;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Services;

namespace Talkift.Client.Views
{
    public sealed partial class MessageContextMenu : MenuFlyout
    {
        public string MessageId { get; set; } = string.Empty;

        public event EventHandler<string>? ActionRequested;

        public MessageContextMenu()
        {
            this.InitializeComponent();
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            CopyItem.Text = LanguageService.GetString("Copy");
            ReplyItem.Text = LanguageService.GetString("ReplyTo");
            ForwardItem.Text = LanguageService.GetString("Forward");
            PinItem.Text = LanguageService.GetString("Pin");
            EditItem.Text = LanguageService.GetString("Edit");
            DeleteItem.Text = LanguageService.GetString("Delete");
        }

        private void MenuItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is string action)
            {
                ActionRequested?.Invoke(this, action);
            }
        }
    }
}
