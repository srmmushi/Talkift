using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Talkift.Client.Services;
using Talkift.Client.ViewModels.V2;

namespace Talkift.Client.Views.V2;

public sealed partial class SearchPanel : Page
{
    public SearchViewModel ViewModel { get; set; } = new(null!, null!, null!);

    public SearchPanel()
    {
        this.InitializeComponent();
        this.Loaded += SearchPanel_Loaded;
    }

    private void SearchPanel_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel = new SearchViewModel(
                App.ChatEngine!,
                App.MessageStore!,
                App.LoggerService!
            );
            DataContext = ViewModel;
        }
        catch (Exception ex)
        {
            CrashLogger.LogException("SearchPanel_Loaded", ex);
        }
    }

    private void SearchInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrWhiteSpace(SearchInput.Text))
        {
            _ = ViewModel.SearchAsync();
            ResultCountText.Text = $"{ViewModel.ResultCount} results";
        }
    }

    private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SearchInput.Text))
        {
            ViewModel.ClearSearch();
            ResultCountText.Text = string.Empty;
        }
    }

    private void TypeFilterButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            _ = ViewModel.FilterByTypeAsync(btn.Content?.ToString() ?? "all");
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Visibility = Visibility.Collapsed;
    }
}
