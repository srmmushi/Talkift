using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Talkift.Client.Models;
using Talkift.Client.Services;

namespace Talkift.Client.ViewModels
{
    public partial class ChatViewModel : ViewModelBase
    {
        private readonly WebSocketService _wsService = new();
        private string _currentUserId = string.Empty;
        private bool _isSubscribed;
        private DispatcherQueue? _dispatcherQueue;

        [ObservableProperty]
        private Server? _currentServer;

        [ObservableProperty]
        private Conversation? _currentConversation;

        [ObservableProperty]
        private string _messageText = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isConnected;

        [ObservableProperty]
        private string _chatTitle = string.Empty;

        [ObservableProperty]
        private string _memberCountText = string.Empty;

        public ObservableCollection<ChatMessage> Messages { get; } = new();

        public event Action<ChatMessage>? MessageReceived;
        public event Action<ChatMessage>? NewMessageReceived;
        public event Action<string>? ErrorOccurred;
        public event Action<string>? UserJoined;

        public WebSocketService WsService => _wsService;

        public async Task InitializeAsync(Server server, Conversation conversation, string userId)
        {
            _currentUserId = userId;
            CurrentServer = server;
            CurrentConversation = conversation;
            ChatTitle = conversation.Name;
            MemberCountText = conversation.IsGroup ? string.Format(LanguageService.GetString("MembersCount"), conversation.Members.Count) : "";
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            if (!_isSubscribed)
            {
                _wsService.MessageReceived += OnRawMessage;
                _wsService.NewMessageReceived += OnIncomingMessage;
                _wsService.Connected += OnConnected;
                _wsService.Disconnected += OnDisconnected;
                _wsService.ErrorOccurred += OnWsError;
                _wsService.UserJoined += OnWsUserJoined;
                _isSubscribed = true;
            }

            if (!_wsService.IsConnected)
            {
                var url = ServerService.BuildWebSocketUrl(server.Address, server.Port);
                await _wsService.ConnectAsync(url, userId: userId);
            }

            await LoadHistoryAsync();
        }

        private void OnRawMessage(string raw)
        {
        }

        private void OnConnected()
        {
            IsConnected = true;
        }

        private void OnDisconnected()
        {
            IsConnected = false;
        }

        private void OnWsError(string msg)
        {
            ErrorOccurred?.Invoke(msg);
        }

        private void OnWsUserJoined(string username)
        {
            UserJoined?.Invoke(username);
        }

        private void OnIncomingMessage(string conversationId, ChatMessage message)
        {
            if (conversationId == CurrentConversation?.Id)
            {
                AddMessage(message);
                NewMessageReceived?.Invoke(message);
            }
        }

        public async Task LoadHistoryAsync()
        {
            if (CurrentConversation == null || CurrentServer == null)
                return;

            IsLoading = true;
            try
            {
                await _wsService.SendLoadHistoryAsync(CurrentConversation.Id);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(MessageText) || CurrentConversation == null)
                return;

            var content = MessageText;
            MessageText = string.Empty;

            await _wsService.SendChatAsync(CurrentConversation.Id, content);
        }

        public void AddMessage(ChatMessage message)
        {
            void Update()
            {
                if (message.ConversationId == CurrentConversation?.Id)
                {
                    message.IsMine = message.SenderId == _currentUserId;
                    message.TimeDisplay = FormatTimestamp(message.Timestamp);
                    Messages.Add(message);

                    if (CurrentConversation != null)
                    {
                        CurrentConversation.LastMessage = message.Content;
                        CurrentConversation.LastMessageTime = message.TimeDisplay;
                    }
                }
            }

            if (_dispatcherQueue != null)
                _dispatcherQueue.TryEnqueue(Update);
            else
                Update();
        }

        public void LoadHistoryFromServer(System.Collections.Generic.List<ChatMessage> historyMessages)
        {
            Messages.Clear();
            foreach (var msg in historyMessages)
            {
                msg.IsMine = msg.SenderId == _currentUserId;
                msg.TimeDisplay = FormatTimestamp(msg.Timestamp);
                Messages.Add(msg);
            }

            if (Messages.Count > 0 && CurrentConversation != null)
            {
                var last = Messages.Last();
                CurrentConversation.LastMessage = last.Content;
                CurrentConversation.LastMessageTime = last.TimeDisplay;
            }
        }

        public void UpdateConversationInfo(Conversation conversation)
        {
            CurrentConversation = conversation;
            ChatTitle = conversation.Name;
            if (conversation.IsGroup)
                MemberCountText = string.Format(LanguageService.GetString("MembersCount"), conversation.Members.Count);
        }

        public async Task DisconnectAsync()
        {
            if (_isSubscribed)
            {
                _wsService.MessageReceived -= OnRawMessage;
                _wsService.NewMessageReceived -= OnIncomingMessage;
                _wsService.Connected -= OnConnected;
                _wsService.Disconnected -= OnDisconnected;
                _wsService.ErrorOccurred -= OnWsError;
                _wsService.UserJoined -= OnWsUserJoined;
                _isSubscribed = false;
            }
            await _wsService.DisconnectAsync();
        }

        private static string FormatTimestamp(long timestamp)
        {
            var dt = DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
            var now = DateTime.Now;
            if (dt.Date == now.Date)
                return dt.ToString("HH:mm");
            if (dt.Date == now.Date.AddDays(-1))
                return LanguageService.GetString("Yesterday") + " " + dt.ToString("HH:mm");
            return dt.ToString("MM/dd HH:mm");
        }

        public System.Collections.Generic.List<ChatMessage> SearchMessages(string query)
        {
            var results = new System.Collections.Generic.List<ChatMessage>();
            foreach (var msg in Messages)
            {
                if (msg.Content != null && msg.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(msg);
                }
            }
            return results;
        }

        public System.Collections.Generic.List<string> GetPinnedMessages()
        {
            var pinned = new System.Collections.Generic.List<string>();
            foreach (var msg in Messages)
            {
                if (msg.IsPinned)
                {
                    pinned.Add($"[{msg.SenderName}] {msg.Content}");
                }
            }
            return pinned;
        }
    }
}
