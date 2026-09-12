using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Talkift.Client.Views.Controls;

public sealed partial class EmptyStateControl : UserControl
{
    public EmptyStateControl()
    {
        this.InitializeComponent();
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        EmptyTitle.Text = LanguageService.GetString("NoMessages");
        EmptySubtitle.Text = LanguageService.GetString("NoConversations");
        ActionButton.Content = LanguageService.GetString("CreateChat");
    }

    public void SetContent(string title, string subtitle, string actionText)
    {
        EmptyTitle.Text = title;
        EmptySubtitle.Text = subtitle;
        ActionButton.Content = actionText;
    }

    public event EventHandler ActionButtonClick;

    private void ActionButton_Click(object sender, RoutedEventArgs e)
    {
        ActionButtonClick?.Invoke(this, EventArgs.Empty);
    }
}
