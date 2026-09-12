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

public partial class ContactViewModel : ObservableObject, IDisposable
{
    private readonly IChatEngine _chatEngine;
    private readonly IAuthService _authService;
    private readonly ILoggerService _loggerService;
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _disposed;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private ObservableCollection<UserProfileModel> _contacts = new();

    [ObservableProperty]
    private ObservableCollection<UserProfileModel> _groupMembers = new();

    [ObservableProperty]
    private UserProfileModel? _selectedContact;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isGroup;

    [ObservableProperty]
    private string _groupName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _selectedMembers = new();

    [ObservableProperty]
    private string _groupDescription = string.Empty;

    [ObservableProperty]
    private bool _isMuted;

    [ObservableProperty]
    private bool _isBlocked;

    public ContactViewModel(
        IChatEngine chatEngine,
        IAuthService authService,
        ILoggerService loggerService)
    {
        _chatEngine = chatEngine ?? throw new ArgumentNullException(nameof(chatEngine));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    [RelayCommand]
    private async Task LoadContactsAsync()
    {
        try
        {
            _contacts.Clear();
            // Load from server
        }
        catch (Exception ex)
        {
            _loggerService.Error(nameof(ContactViewModel), "Load failed", ex);
        }
    }

    [RelayCommand]
    private void AddContact(UserProfileModel contact)
    {
        if (!_contacts.Any(c => c.Id == contact.Id))
            _contacts.Add(contact);
    }

    [RelayCommand]
    private void RemoveContact(UserProfileModel contact)
    {
        _contacts.Remove(contact);
    }

    [RelayCommand]
    private async Task MuteContactAsync()
    {
        _isMuted = true;
    }

    [RelayCommand]
    private async Task UnmuteContactAsync()
    {
        _isMuted = false;
    }

    [RelayCommand]
    private async Task BlockContactAsync()
    {
        _isBlocked = true;
    }

    [RelayCommand]
    private async Task UnblockContactAsync()
    {
        _isBlocked = false;
    }

    [RelayCommand]
    private async Task StartGroupAsync()
    {
        _isGroup = true;
        await _chatEngine.CreateGroupAsync(_groupName, _selectedMembers.ToList(), _cts.Token);
    }

    [RelayCommand]
    private async Task AddGroupMemberAsync(UserProfileModel member)
    {
        if (!_selectedMembers.Contains(member.Id))
        {
            _selectedMembers.Add(member.Id);
            _groupMembers.Add(member);
        }
    }

    [RelayCommand]
    private void RemoveGroupMember(UserProfileModel member)
    {
        _selectedMembers.Remove(member.Id);
        _groupMembers.Remove(member);
    }

    [RelayCommand]
    private async Task LeaveGroupAsync()
    {
        // Leave group implementation
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
