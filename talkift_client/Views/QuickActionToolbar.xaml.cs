using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Talkift.Client.Views
{
    public sealed partial class QuickActionToolbar : UserControl
    {
        public event EventHandler? EmojiClicked;
        public event EventHandler? FileClicked;
        public event EventHandler? ImageClicked;
        public event EventHandler? VoiceClicked;

        public QuickActionToolbar()
        {
            this.InitializeComponent();
        }

        private void EmojiButton_Click(object sender, RoutedEventArgs e) => EmojiClicked?.Invoke(this, EventArgs.Empty);
        private void FileButton_Click(object sender, RoutedEventArgs e) => FileClicked?.Invoke(this, EventArgs.Empty);
        private void ImageAttachButton_Click(object sender, RoutedEventArgs e) => ImageClicked?.Invoke(this, EventArgs.Empty);
        private void VoiceButton_Click(object sender, RoutedEventArgs e) => VoiceClicked?.Invoke(this, EventArgs.Empty);
    }
}
