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

public partial class MessageListViewModel : ObservableObject
{
    private readonly IChatEngine _chatEngine;
    private readonly IMessageStore _messageStore;
    private readonly ILoggerService _loggerService;
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _disposed;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private ObservableCollection<ChatMessageModel> _messages = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ChatMessageModel> _filteredMessages;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private string _errorText = string.Empty;

    [ObservableProperty]
    private int _unreadCount;

    [ObservableProperty]
    private string _conversationId = string.Empty;

    [ObservableProperty]
    private ConversationModel? _conversation;

    public MessageListViewModel(
        IChatEngine chatEngine,
        IMessageStore messageStore,
        ILoggerService loggerService)
    {
        _chatEngine = chatEngine ?? throw new ArgumentNullException(nameof(chatEngine));
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        FilteredMessages = new ObservableCollection<ChatMessageModel>();
    }

    public async Task InitializeAsync(string conversationId, string? before = null)
    {
        _conversationId = conversationId;
        _isLoading = true;
        _cts = new CancellationTokenSource();

        try
        {
            var msgs = await _messageStore.GetMessagesAsync(conversationId, limit: 50, before: before, ct: _cts.Token);
            Messages.Clear();
            foreach (var msg in msgs)
                Messages.Add(msg);

            _isLoading = false;
            _isEmpty = Messages.Count == 0;
        }
        catch (Exception ex)
        {
            _loggerService.Error(nameof(MessageListViewModel), "Load failed", ex);
            ErrorText = "Failed to load messages";
            _isLoading = false;
        }
    }

    [RelayCommand]
    private void AddMessage(ChatMessageModel message)
    {
        Messages.Add(message);
        FilteredMessages.Add(message);

        if (!string.IsNullOrEmpty(SearchQuery))
            FilterMessages();
    }

    [RelayCommand]
    private void FilterMessages()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            FilteredMessages.Clear();
            foreach (var msg in Messages)
                FilteredMessages.Add(msg);
        }
        else
        {
            FilteredMessages.Clear();
            foreach (var msg in Messages)
            {
                if (msg.Content.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                    FilteredMessages.Add(msg);
            }
        }
    }

    [RelayCommand]
    private async Task LoadOlderAsync()
    {
        if (Messages.Count == 0) return;
        var oldestId = Messages[0].Id;
        var more = await _messageStore.GetMessagesAsync(_conversationId, limit: 20, before: oldestId, ct: _cts.Token);
        foreach (var msg in more)
            Messages.Insert(0, msg);
    }

    [RelayCommand]
    private void MarkAsRead(string messageId)
    {
        _chatEngine.SendReadReceiptAsync(_conversationId, messageId, _cts.Token).GetAwaiter().GetResult();
        var msg = Messages.FirstOrDefault(m => m.Id == messageId);
        if (msg != null) msg.Status = MessageStatus.Read;
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
