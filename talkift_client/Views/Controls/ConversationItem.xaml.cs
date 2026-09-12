using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Talkift.Client.Models.V2;

namespace Talkift.Client.Views.Controls;

public sealed partial class ConversationItem : UserControl
{
    public static readonly DependencyProperty ModelProperty =
        DependencyProperty.Register(nameof(Model), typeof(ConversationModel), typeof(ConversationItem),
            new PropertyMetadata(null));

    public ConversationModel Model
    {
        get => (ConversationModel)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    public ConversationItem()
    {
        this.InitializeComponent();
        this.PointerPressed += ConversationItem_PointerPressed;
    }

    private void ConversationItem_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        // Selection handled by parent ListView
    }

    private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        // Hover visual feedback
        var grid = sender as Grid;
        if (e.GetCurrentPoint(grid).Properties.IsLeftButtonPressed)
        {
            grid.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.ColorHelper.FromArgb(40, 120, 120, 120));
        }
        else
        {
            grid.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.ColorHelper.FromArgb(0, 0, 0, 0));
        }
    }
}
