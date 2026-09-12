using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Talkift.Client.Engines;
using Talkift.Client.Models.V2;
using Talkift.Client.Services;

namespace Talkift.Client.ViewModels.V2;

public partial class SearchViewModel : ObservableObject, IDisposable
{
    private readonly IChatEngine _chatEngine;
    private readonly IMessageStore _messageStore;
    private readonly ILoggerService _loggerService;
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _disposed;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private string _query = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ChatMessageModel> _results = new();

    [ObservableProperty]
    private int _resultCount;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private string _conversationId = string.Empty;

    [ObservableProperty]
    private string _filterType = "all";

    [ObservableProperty]
    private bool _isCaseSensitive;

    public SearchViewModel(
        IChatEngine chatEngine,
        IMessageStore messageStore,
        ILoggerService loggerService)
    {
        _chatEngine = chatEngine ?? throw new ArgumentNullException(nameof(chatEngine));
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    public async Task InitializeAsync(string conversationId)
    {
        _conversationId = conversationId;
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(Query)) return;

        _isSearching = true;
        _cts = new CancellationTokenSource();

        try
        {
            var msgs = await _messageStore.GetMessagesAsync(
                _conversationId, limit: 100, ct: _cts.Token);

            Results.Clear();
            foreach (var msg in msgs)
            {
                if (msg.Content.Contains(Query, _isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                {
                    Results.Add(msg);
                }
            }

            ResultCount = Results.Count;
        }
        catch (Exception ex)
        {
            _loggerService.Error(nameof(SearchViewModel), "Search failed", ex);
        }
        finally
        {
            _isSearching = false;
        }
    }

    [RelayCommand]
    private void ClearSearch()
    {
        Query = string.Empty;
        Results.Clear();
        ResultCount = 0;
    }

    [RelayCommand]
    private async Task FilterByTypeAsync(string type)
    {
        _filterType = type;
        var filtered = Results.Where(m => m.Type.ToLower() == type.ToLower()).ToList();
        Results.Clear();
        foreach (var msg in filtered)
            Results.Add(msg);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cts?.Cancel();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
