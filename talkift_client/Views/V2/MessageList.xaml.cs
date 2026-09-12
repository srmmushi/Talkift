using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.ViewModels.V2;

namespace Talkift.Client.Views.V2;

public sealed partial class MessageListPage : Page
{
    public MessageListViewModel ViewModel { get; } = new(null!, null!, null!);

    public MessageListPage()
    {
        this.InitializeComponent();
        this.Loaded += MessageListPage_Loaded;
    }

    private void MessageListPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel = new MessageListViewModel(
                App.ChatEngine!,
                App.MessageStore!,
                App.LoggerService!
            );
            DataContext = ViewModel;
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("MessageListPage_Loaded", ex);
        }
    }
}
