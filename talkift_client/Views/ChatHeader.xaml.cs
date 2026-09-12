using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Talkift.Client.Views
{
    public sealed partial class ChatHeader : UserControl
    {
        public event EventHandler? ChatClicked;
        public event EventHandler? SearchClicked;
        public event EventHandler? PinClicked;
        public event EventHandler? MembersClicked;
        public event EventHandler? MoreClicked;

        public ChatHeader()
        {
            this.InitializeComponent();
        }

        public void SetChatInfo(string name, string status)
        {
            ChatNameText.Text = name;
            ChatStatusText.Text = status;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e) => SearchClicked?.Invoke(this, EventArgs.Empty);
        private void PinButton_Click(object sender, RoutedEventArgs e) => PinClicked?.Invoke(this, EventArgs.Empty);
        private void MembersButton_Click(object sender, RoutedEventArgs e) => MembersClicked?.Invoke(this, EventArgs.Empty);
        private void MoreButton_Click(object sender, RoutedEventArgs e) => MoreClicked?.Invoke(this, EventArgs.Empty);
        private void ChatArea_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e) => ChatClicked?.Invoke(this, EventArgs.Empty);
    }
}
