using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Services;

namespace Talkift.Client.Views
{
    public sealed partial class CreateConversationDialog : ContentDialog
    {
        public WebSocketService WebSocketService { get; set; } = null!;

        private readonly List<string> _allMembers = new();

        public CreateConversationDialog()
        {
            this.InitializeComponent();
            this.Loaded += CreateConversationDialog_Loaded;
        }

        private async void CreateConversationDialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await WebSocketService.SendLoadMembersAsync("public-group");

            WebSocketService.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(string raw)
        {
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(raw);
                var root = doc.RootElement;

                if (root.TryGetProperty("type", out var type) && type.GetString() == "members_list")
                {
                    if (root.TryGetProperty("payload", out var payload) && payload.TryGetProperty("members", out var members))
                    {
                        _allMembers.Clear();
                        foreach (var m in members.EnumerateArray())
                        {
                            var username = m.GetProperty("username").GetString() ?? "";
                            if (!string.IsNullOrEmpty(username))
                                _allMembers.Add(username);
                        }

                        DispatcherQueue.TryEnqueue(() =>
                        {
                            MemberListView.ItemsSource = _allMembers;
                        });
                    }
                }
            }
            catch (System.Text.Json.JsonException)
            {
            }
        }

        private void ConvNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateForm();
        }

        private void MemberListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateForm();
        }

        private void ValidateForm()
        {
            IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(ConvNameInput.Text)
                && MemberListView.SelectedItem != null;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;

            var name = ConvNameInput.Text.Trim();
            var targetUsername = MemberListView.SelectedItem as string ?? "";

            try
            {
                await WebSocketService.SendCreateConversationAsync(name, targetUsername);
                WebSocketService.MessageReceived -= OnMessageReceived;
                Hide();
            }
            catch (Exception ex)
            {
                ErrorBar.Message = ex.Message;
                ErrorBar.IsOpen = true;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            WebSocketService.MessageReceived -= OnMessageReceived;
        }
    }
}
