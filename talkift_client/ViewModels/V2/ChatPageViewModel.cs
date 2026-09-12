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

public partial class ChatPageViewModel : ObservableObject, IDisposable
{
    private readonly IChatEngine _chatEngine;
    private readonly IUiEngine _uiEngine;
    private readonly IMessageStore _messageStore;
    private readonly INotificationService _notificationService;
    private readonly ILoggerService _loggerService;
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _disposed;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private string _chatTitle = string.Empty;

    [ObservableProperty]
    private string _memberCountText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ChatMessageModel> _messages = new();

    [ObservableProperty]
    private string _messageText = string.Empty;

    [ObservableProperty]
    private bool _isSending;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _connectionStatus = "Disconnected";

    [ObservableProperty]
    private ConversationModel? _currentConversation;

    [ObservableProperty]
    private string _replyToMessageId = string.Empty;

    [ObservableProperty]
    private string _replyToSenderName = string.Empty;

    [ObservableProperty]
    private bool _isReplyBarVisible;

    [ObservableProperty]
    private bool _isTypingIndicatorVisible;

    [ObservableProperty]
    private string _typingUser = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _pinnedMessages = new();

    [ObservableProperty]
    private bool _isSearchPanelVisible;

    [ObservableProperty]
    private int _searchResultCount;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _showScrollToBottom;

    public ChatPageViewModel(
        IChatEngine chatEngine,
        IUiEngine uiEngine,
        IMessageStore messageStore,
        INotificationService notificationService,
        ILoggerService loggerService)
    {
        _chatEngine = chatEngine ?? throw new ArgumentNullException(nameof(chatEngine));
        _uiEngine = uiEngine ?? throw new ArgumentNullException(nameof(uiEngine));
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _cts = new CancellationTokenSource();

        _chatEngine.MessageReceived += OnMessageReceived;
        _chatEngine.TypingReceived += OnTypingReceived;
        _chatEngine.ConnectionStateChanged += OnConnectionStateChanged;
    }

    public async Task InitializeAsync(Server server, ConversationModel conversation, string userId)
    {
        if (conversation == null) throw new ArgumentNullException(nameof(conversation));

        _currentConversation = conversation;
        ChatTitle = conversation.Name;
        MemberCountText = conversation.IsGroup
            ? $"{conversation.Members.Count} members"
            : string.Empty;

        _isConnected = _chatEngine.IsConnected;
        ConnectionStatus = _chatEngine.State.ToString();

        var history = await _messageStore.GetMessagesAsync(conversation.Id, limit: 50, ct: _cts.Token);
        foreach (var msg in history)
        {
            msg.IsMine = msg.SenderId == userId;
            Messages.Add(msg);
        }

        _chatEngine.LoadHistoryAsync(conversation.Id, ct: _cts.Token);
    }

    private void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            var msg = new ChatMessageModel
            {
                Id = e.MessageId,
                SenderId = e.SenderId,
                SenderName = e.SenderName,
                Content = e.Content,
                Timestamp = e.Timestamp,
                Type = e.Type,
                ConversationId = e.ConversationId,
                IsMine = e.SenderId == "current_user",
                TimeDisplay = new ChatMessageModel
                {
                    Timestamp = e.Timestamp
                }.GetFormattedTime()
            };

            Messages.Add(msg);
            _currentConversation!.LastMessage = e.Content;

            if (Messages.Count > 0)
                ShowScrollToBottom = true;
        });
    }

    private void OnTypingReceived(object? sender, TypingReceivedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            IsTypingIndicatorVisible = e.IsTyping;
            TypingUser = e.Username;
        });
    }

    private void OnConnectionStateChanged(object? sender, ConnectionStateChangedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            IsConnected = e.NewState == ConnectionState.Connected;
            ConnectionStatus = e.NewState.ToString();
        });
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageText) || _currentConversation == null)
            return;

        var text = MessageText;
        if (!string.IsNullOrEmpty(_replyToMessageId))
        {
            text = $"[Reply to {_replyToSenderName}]: {text}";
            _replyToMessageId = string.Empty;
            _replyToSenderName = string.Empty;
            _isReplyBarVisible = false;
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            IsSending = true;
            var msg = new ChatMessageModel
            {
                Id = Guid.NewGuid().ToString("N"),
                SenderId = "current_user",
                SenderName = "You",
                Content = text,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Type = "chat",
                ConversationId = _currentConversation.Id,
                IsMine = true,
                TimeDisplay = DateTime.Now.ToString("HH:mm")
            };

            Messages.Add(msg);
            MessageText = string.Empty;
            ShowScrollToBottom = true;

            try
            {
                await _chatEngine.SendMessageAsync(_currentConversation.Id, text, _cts.Token);
                _notificationService.ShowSuccessAsync("Message sent").GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _loggerService.Error(nameof(ChatPageViewModel), "Send failed", ex);
                _notificationService.ShowErrorAsync("Failed to send message").GetAwaiter().GetResult();
            }
            finally
            {
                IsSending = false;
            }
        }
    }

    [RelayCommand]
    private void CancelReply()
    {
        _replyToMessageId = string.Empty;
        _replyToSenderName = string.Empty;
        _isReplyBarVisible = false;
    }

    [RelayCommand]
    private void ShowReply(string messageId, string senderName)
    {
        _replyToMessageId = messageId;
        _replyToSenderName = senderName;
        _isReplyBarVisible = true;
    }

    [RelayCommand]
    private async Task SendReactionAsync(string emoji)
    {
        if (_currentConversation == null || string.IsNullOrEmpty(_replyToMessageId))
            return;

        try
        {
            await _chatEngine.SendReactionAsync(_replyToMessageId, _currentConversation.Id, emoji, _cts.Token);
        }
        catch (Exception ex)
        {
            _loggerService.Error(nameof(ChatPageViewModel), "Reaction failed", ex);
        }
    }

    [RelayCommand]
    private async Task SendTypingAsync()
    {
        if (_currentConversation == null) return;
        try
        {
            await _chatEngine.SendTypingAsync(_currentConversation.Id, true, _cts.Token);
        }
        catch { }
    }

    [RelayCommand]
    private async Task SearchMessagesAsync(string query)
    {
        if (_currentConversation == null) return;
        var results = Messages.Where(m => m.Content.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        SearchResultCount = results.Count;
    }

    [RelayCommand]
    private void ShowPinnedMessages()
    {
        if (_currentConversation == null) return;
        var pinned = Messages.Where(m => m.IsPinned).ToList();
        PinnedMessages.Clear();
        foreach (var msg in pinned)
            PinnedMessages.Add($"[{msg.SenderName}] {msg.Content}");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _chatEngine.MessageReceived -= OnMessageReceived;
        _chatEngine.TypingReceived -= OnTypingReceived;
        _chatEngine.ConnectionStateChanged -= OnConnectionStateChanged;

        GC.SuppressFinalize(this);
    }
}
