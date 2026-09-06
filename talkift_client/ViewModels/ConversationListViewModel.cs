using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Talkift.Client.Models;
using Talkift.Client.Services;

namespace Talkift.Client.ViewModels
{
    public partial class ConversationListViewModel : ViewModelBase
    {
        private readonly WebSocketService _wsService = new();
        private string _currentUserId = string.Empty;
        private string _currentUsername = string.Empty;

        [ObservableProperty]
        private Server? _currentServer;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isConnected;

        [ObservableProperty]
        private string _connectionStatus = "Disconnected";

        public ObservableCollection<Conversation> Conversations { get; } = new();

        public Dictionary<string, int> UnreadCounts { get; } = new();

        public event Action<Conversation>? ConversationSelected;
        public event Action<Conversation>? ConversationUpdated;
        public event Action<string>? ErrorOccurred;
        public event Action<string, string>? JoinRequestReceived;

        public WebSocketService WsService => _wsService;

        public async Task InitializeAsync(Server server, string userId, string username)
        {
            CurrentServer = server;
            _currentUserId = userId;
            _currentUsername = username;

            _wsService.Connected += OnConnected;
            _wsService.Disconnected += OnDisconnected;
            _wsService.ErrorOccurred += msg => ErrorOccurred?.Invoke(msg);
            _wsService.NewMessageReceived += OnNewMessage;
            _wsService.ConversationUpdated += OnConversationUpdated;
            _wsService.JoinRequestReceived += (from, group) => JoinRequestReceived?.Invoke(from, group);

            if (!_wsService.IsConnected)
            {
                var url = $"ws://{server.Address}:{server.Port}/ws";
                await _wsService.ConnectAsync(url, userId: userId);
            }

            ConnectionStatus = "Connected";
            IsConnected = true;
        }

        private void OnConnected()
        {
            IsConnected = true;
            ConnectionStatus = "Connected";
        }

        private void OnDisconnected()
        {
            IsConnected = false;
            ConnectionStatus = "Disconnected";
        }

        private void OnNewMessage(string conversationId, ChatMessage message)
        {
            var conv = Conversations.FirstOrDefault(c => c.Id == conversationId);
            if (conv != null)
            {
                conv.LastMessage = message.Content;
                conv.LastMessageTime = message.TimeDisplay;

                if (!message.IsMine)
                {
                    conv.UnreadCount++;
                    UnreadCounts[conversationId] = conv.UnreadCount;
                }

                Conversations.Move(Conversations.IndexOf(conv), 0);
            }
        }

        private void OnConversationUpdated(Conversation conversation)
        {
            var existing = Conversations.FirstOrDefault(c => c.Id == conversation.Id);
            if (existing != null)
            {
                var idx = Conversations.IndexOf(existing);
                Conversations[idx] = conversation;
            }
            else
            {
                Conversations.Insert(0, conversation);
            }

            ConversationUpdated?.Invoke(conversation);
        }

        public void MarkAsRead(string conversationId)
        {
            var conv = Conversations.FirstOrDefault(c => c.Id == conversationId);
            if (conv != null)
            {
                conv.UnreadCount = 0;
                UnreadCounts[conversationId] = 0;
            }
        }

        public void ClearUnread(string conversationId)
        {
            MarkAsRead(conversationId);
        }

        [RelayCommand]
        private async Task LoadConversationsAsync()
        {
            if (CurrentServer == null)
                return;

            IsLoading = true;
            try
            {
                await _wsService.SendLoadConversationsAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void SelectConversation(Conversation conversation)
        {
            ConversationSelected?.Invoke(conversation);
        }

        [RelayCommand]
        private async Task CreateGroupAsync(string name, List<string> members)
        {
            if (CurrentServer == null)
                return;
            await _wsService.SendCreateGroupAsync(name, members);
        }

        [RelayCommand]
        private async Task CreateConversationAsync(string name, string targetUsername)
        {
            if (CurrentServer == null)
                return;
            await _wsService.SendCreateConversationAsync(name, targetUsername);
        }

        public async Task DisconnectAsync()
        {
            _wsService.Connected -= OnConnected;
            _wsService.Disconnected -= OnDisconnected;
            _wsService.ErrorOccurred -= msg => ErrorOccurred?.Invoke(msg);
            _wsService.NewMessageReceived -= OnNewMessage;
            _wsService.ConversationUpdated -= OnConversationUpdated;
            _wsService.JoinRequestReceived -= (from, group) => JoinRequestReceived?.Invoke(from, group);
        }
    }
}
