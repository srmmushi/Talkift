using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Services;

namespace Talkift.Client.Views
{
    public sealed partial class PinMessagePanel : UserControl
    {
        private readonly ObservableCollection<string> _pinnedMessages = new();

        public PinMessagePanel()
        {
            this.InitializeComponent();
            ApplyLocalization();
            PinnedMessagesList.ItemsSource = _pinnedMessages;
        }

        private void ApplyLocalization()
        {
            TitleText.Text = LanguageService.GetString("PinnedMessages");
        }

        public void Show(List<string> messages)
        {
            _pinnedMessages.Clear();
            foreach (var msg in messages)
                _pinnedMessages.Add(msg);
            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();
    }
}
