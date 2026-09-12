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

public partial class ConversationListViewModel : ObservableObject, IDisposable
{
    private readonly IChatEngine _chatEngine;
    private readonly IAuthService _authService;
    private readonly IMessageStore _messageStore;
    private readonly ILoggerService _loggerService;
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _disposed;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private ObservableCollection<ConversationModel> _conversations = new();

    [ObservableProperty]
    private ConversationModel? _selectedConversation;

    [ObservableProperty]
    private string _connectionStatus = "Disconnected";

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _filterText = string.Empty;

    public ConversationListViewModel(
        IChatEngine chatEngine,
        IAuthService authService,
        IMessageStore messageStore,
        ILoggerService loggerService)
    {
        _chatEngine = chatEngine ?? throw new ArgumentNullException(nameof(chatEngine));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _cts = new CancellationTokenSource();

        _chatEngine.ConversationUpdated += OnConversationUpdated;
        _chatEngine.JoinRequestReceived += OnJoinRequestReceived;
        _chatEngine.ConnectionStateChanged += OnConnectionStateChanged;
    }

    public async Task InitializeAsync(Server server, string userId, string username)
    {
        _isLoading = true;
        try
        {
            await _chatEngine.InitializeAsync(
                new ChatEngineOptions { ServerAddress = server.Address, ServerPort = server.Port, UserId = userId },
                _cts.Token);

            await _chatEngine.LoadConversationsAsync(_cts.Token);
        }
        catch (Exception ex)
        {
            _loggerService.Error(nameof(ConversationListViewModel), "Init failed", ex);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void OnConversationUpdated(object? sender, ConversationUpdatedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            var conv = Conversations.FirstOrDefault(c => c.Id == e.ConversationId);
            if (conv != null)
            {
                conv.Name = e.Name;
                conv.IsGroup = e.IsGroup;
                conv.LastMessage = e.LastMessage;
                conv.LastMessageTimestamp = e.LastMessageTimestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                conv.Members = e.Members;

                if (!e.IsMine && e.LastMessage != null)
                    conv.UnreadCount++;

                var idx = Conversations.IndexOf(conv);
                if (idx > 0)
                    Conversations.Move(idx, 0);
            }
            else
            {
                var newConv = new ConversationModel
                {
                    Id = e.ConversationId,
                    Name = e.Name,
                    IsGroup = e.IsGroup,
                    Members = e.Members,
                    LastMessage = e.LastMessage,
                    LastMessageTimestamp = e.LastMessageTimestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    IsMuted = false
                };
                Conversations.Insert(0, newConv);
            }
        });
    }

    private void OnJoinRequestReceived(object? sender, JoinRequestEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            _loggerService.Info(nameof(ConversationListViewModel), $"Join request from {e.FromUsername} for {e.GroupName}");
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
    private void SelectConversation(ConversationModel conversation)
    {
        SelectedConversation = conversation;
        conversation.UnreadCount = 0;
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (_selectedConversation == null) return;
        var oldest = Messages.Count > 0 ? Messages[0].Id : null;
        var more = await _messageStore.GetMessagesAsync(
            _selectedConversation.Id, limit: 20, before: oldest, ct: _cts.Token);
        foreach (var msg in more)
            Messages.Insert(0, msg);
    }

    [RelayCommand]
    private void FilterConversations()
    {
        if (string.IsNullOrWhiteSpace(FilterText))
        {
            // All conversations visible
        }
    }

    public ObservableCollection<ChatMessageModel> Messages =>
        SelectedConversation != null
            ? new ObservableCollection<ChatMessageModel>(
                MessagesStore.GetMessagesSync(SelectedConversation.Id))
            : new ObservableCollection<ChatMessageModel>();

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _chatEngine.ConversationUpdated -= OnConversationUpdated;
        _chatEngine.JoinRequestReceived -= OnJoinRequestReceived;
        _chatEngine.ConnectionStateChanged -= OnConnectionStateChanged;

        GC.SuppressFinalize(this);
    }
}
