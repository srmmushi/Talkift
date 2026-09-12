using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Services;

namespace Talkift.Client.Views
{
    public sealed partial class TypingIndicator : UserControl
    {
        private string _typingUser = string.Empty;

        public TypingIndicator()
        {
            this.InitializeComponent();
        }

        public void Show(string username)
        {
            _typingUser = username;
            TypingText.Text = string.Format(LanguageService.GetString("UserTyping"), username);
            this.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            _typingUser = string.Empty;
        }
    }
}
