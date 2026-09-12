using System;
using System.Windows;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Animation;
using Talkift.Client.Models.V2;

namespace Talkift.Client.Views.Controls;

public sealed partial class MessageBubble : UserControl
{
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(ChatMessageModel), typeof(MessageBubble),
            new PropertyMetadata(null, OnMessageChanged));

    public static readonly DependencyProperty IsMineProperty =
        DependencyProperty.Register(nameof(IsMine), typeof(bool), typeof(MessageBubble),
            new PropertyMetadata(false, OnIsMineChanged));

    public static readonly DependencyProperty IsPinnedProperty =
        DependencyProperty.Register(nameof(IsPinned), typeof(bool), typeof(MessageBubble),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsEditedProperty =
        DependencyProperty.Register(nameof(IsEdited), typeof(bool), typeof(MessageBubble),
            new PropertyMetadata(false));

    public ChatMessageModel Message
    {
        get => (ChatMessageModel)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public bool IsMine
    {
        get => (bool)GetValue(IsMineProperty);
        set => SetValue(IsMineProperty, value);
    }

    public bool IsPinned
    {
        get => (bool)GetValue(IsPinnedProperty);
        set => SetValue(IsPinnedProperty, value);
    }

    public bool IsEdited
    {
        get => (bool)GetValue(IsEditedProperty);
        set => SetValue(IsEditedProperty, value);
    }

    public MessageBubble()
    {
        this.InitializeComponent();
        this.Loaded += MessageBubble_Loaded;
    }

    private void MessageBubble_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyStyle();
    }

    private void ApplyStyle()
    {
        var bubble = FindName("Bubble") as Border;
        if (bubble == null) return;

        var style = Message.Type?.ToLower() switch
        {
            "system" => (Style)Resources["MessageBubbleSystemStyle"],
            _ => IsMine ? (Style)Resources["MessageBubbleSentStyle"] : (Style)Resources["MessageBubbleReceivedStyle"]
        };
        bubble.Style = style;
    }

    private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MessageBubble bubble)
        {
            bubble.ApplyStyle();
        }
    }

    private static void OnIsMineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MessageBubble bubble)
        {
            bubble.ApplyStyle();
        }
    }
}
