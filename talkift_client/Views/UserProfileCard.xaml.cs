using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Services;

namespace Talkift.Client.Views
{
    public sealed partial class UserProfileCard : UserControl
    {
        public UserProfileCard()
        {
            this.InitializeComponent();
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            EmailLabel.Text = LanguageService.GetString("Email");
            JoinedLabel.Text = LanguageService.GetString("Joined");
        }

        public void SetUser(string username, string email, string status, string joined)
        {
            UsernameText.Text = username;
            EmailText.Text = email;
            StatusText.Text = status;
            JoinedText.Text = joined;
        }
    }
}
