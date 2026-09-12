using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Services;

namespace Talkift.Client.Views.Controls;

public sealed partial class TypingIndicator : UserControl
{
    private string _typingUser = string.Empty;

    public TypingIndicator()
    {
        this.InitializeComponent();
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        TypingText.Text = LanguageService.GetString("UserTyping");
    }

    public void Show(string username)
    {
        _typingUser = username;
        TypingText.Text = string.Format(LanguageService.GetString("UserTyping"), username);
        this.Visibility = Visibility.Visible;
    }

    public void Hide()
    {
        this.Visibility = Visibility.Collapsed;
        _typingUser = string.Empty;
    }
}
