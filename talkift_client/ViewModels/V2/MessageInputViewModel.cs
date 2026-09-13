using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Talkift.Client.Engines;
using Talkift.Client.Services;

namespace Talkift.Client.ViewModels.V2;

public partial class MessageInputViewModel : ObservableObject, IDisposable
{
    private readonly IChatEngine _chatEngine;
    private readonly ILoggerService _loggerService;
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _disposed;

    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private bool _isFocused;

    [ObservableProperty]
    private int _maxLines = 1;

    [ObservableProperty]
    private bool _canSend;

    [ObservableProperty]
    private bool _isComposing;

    [ObservableProperty]
    private string _pendingContent = string.Empty;

    public MessageInputViewModel(
        IChatEngine chatEngine,
        ILoggerService loggerService)
    {
        _chatEngine = chatEngine ?? throw new ArgumentNullException(nameof(chatEngine));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    [RelayCommand]
    public void Send()
    {
        if (!CanSend || string.IsNullOrWhiteSpace(Text)) return;
        CanSend = false;
        _text = string.Empty;
        MaxLines = 1;
        _dispatcherQueue.TryEnqueue(() => CanSend = true);
    }

    [RelayCommand]
    private void AppendText(string text)
    {
        Text += text;
        MaxLines = CalculateMaxLines();
    }

    [RelayCommand]
    private void NewLine()
    {
        _text += "\n";
        MaxLines = CalculateMaxLines();
    }

    [RelayCommand]
    private void Clear()
    {
        Text = string.Empty;
        MaxLines = 1;
    }

    [RelayCommand]
    private void SetFocus()
    {
        IsFocused = true;
    }

    [RelayCommand]
    private void ResetFocus()
    {
        IsFocused = false;
    }

    private int CalculateMaxLines()
    {
        var lines = Text.Split('\n');
        return Math.Clamp(lines.Length, 1, 6);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
