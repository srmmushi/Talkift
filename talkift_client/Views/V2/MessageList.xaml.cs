using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Services;
using Talkift.Client.ViewModels.V2;

namespace Talkift.Client.Views.V2;

public sealed partial class MessageList : Page
{
    public MessageListViewModel ViewModel { get; set; } = new(null!, null!, null!);

    public MessageList()
    {
        this.InitializeComponent();
        this.Loaded += MessageList_Loaded;
    }

    private void MessageList_Loaded(object sender, RoutedEventArgs e)
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
            CrashLogger.LogException("MessageList_Loaded", ex);
        }
    }
}
