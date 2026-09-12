using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Talkift.Client.Services.V2;

public sealed class DialogService : IDialogService
{
    private bool _disposed;

    public async Task<bool> ShowConfirmAsync(string title, string message, string? primaryText = null, string? closeText = null, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        await RunOnUIAsync(() =>
        {
            var window = App.CurrentWindow;
            if (window?.Content == null)
            {
                tcs.TrySetResult(false);
                return;
            }

            _ = DispatcherQueue.GetForCurrentThread()?.TryEnqueue(async () =>
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    PrimaryButtonText = primaryText ?? "OK",
                    CloseButtonText = closeText ?? "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = window.Content.XamlRoot
                };

                try
                {
                    var result = await dialog.ShowAsync();
                    tcs.TrySetResult(result == ContentDialogResult.Primary);
                }
                catch
                {
                    tcs.TrySetResult(false);
                }
            });
        }, ct);

        return await tcs.Task;
    }

    public async Task ShowAsync(string title, string message, string? closeText = null, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        await RunOnUIAsync(() =>
        {
            var window = App.CurrentWindow;
            if (window?.Content == null)
            {
                tcs.TrySetResult(false);
                return;
            }

            _ = DispatcherQueue.GetForCurrentThread()?.TryEnqueue(async () =>
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = closeText ?? "OK",
                    XamlRoot = window.Content.XamlRoot
                };

                try
                {
                    await dialog.ShowAsync();
                }
                catch { }
                tcs.TrySetResult(true);
            });
        }, ct);

        await tcs.Task;
    }

    public async Task<string?> ShowInputAsync(string title, string message, string? placeholder = null, string? defaultValue = null, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

        await RunOnUIAsync(() =>
        {
            var window = App.CurrentWindow;
            if (window?.Content == null)
            {
                tcs.TrySetResult(null);
                return;
            }

            _ = DispatcherQueue.GetForCurrentThread()?.TryEnqueue(async () =>
            {
                var inputBox = new TextBox
                {
                    Text = defaultValue ?? "",
                    PlaceholderText = placeholder ?? "",
                    Width = 300
                };

                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = inputBox,
                    PrimaryButtonText = "OK",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = window.Content.XamlRoot
                };

                try
                {
                    var result = await dialog.ShowAsync();
                    tcs.TrySetResult(result == ContentDialogResult.Primary ? inputBox.Text : null);
                }
                catch
                {
                    tcs.TrySetResult(null);
                }
            });
        }, ct);

        return await tcs.Task;
    }

    private static Task RunOnUIAsync(Action action, CancellationToken ct)
    {
        var window = App.CurrentWindow;
        if (window?.DispatcherQueue == null)
        {
            action();
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        window.DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                action();
                tcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        return tcs.Task;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
