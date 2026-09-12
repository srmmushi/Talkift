using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Models.V2;

namespace Talkift.Client.Views.Controls;

public sealed partial class AvatarControl : UserControl
{
    public static readonly DependencyProperty UsernameProperty =
        DependencyProperty.Register(nameof(Username), typeof(string), typeof(AvatarControl),
            new PropertyMetadata(null, OnUsernameChanged));

    public static readonly DependencyProperty WidthProperty =
        DependencyProperty.Register(nameof(Width), typeof(double), typeof(AvatarControl),
            new PropertyMetadata(36.0));

    public static readonly DependencyProperty HeightProperty =
        DependencyProperty.Register(nameof(Height), typeof(double), typeof(AvatarControl),
            new PropertyMetadata(36.0));

    public string Username
    {
        get => (string)GetValue(UsernameProperty);
        set => SetValue(UsernameProperty, value);
    }

    public double Width
    {
        get => (double)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    public double Height
    {
        get => (double)GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    public AvatarControl()
    {
        this.InitializeComponent();
    }

    private static void OnUsernameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AvatarControl ctrl)
        {
            ctrl.InitialsText.Text = ctrl.GetInitials(ctrl.Username);
        }
    }

    private string GetInitials(string username)
    {
        if (string.IsNullOrEmpty(username)) return "?";
        var parts = username.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();
        return username.Length >= 2 ? username[..2].ToUpper() : username.ToUpper();
    }
}
