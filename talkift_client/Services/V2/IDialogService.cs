using System;
using System.Threading;
using System.Threading.Tasks;

namespace Talkift.Client.Services;

public interface IDialogService : IDisposable
{
    Task<bool> ShowConfirmAsync(string title, string message, string? primaryText = null, string? closeText = null, CancellationToken ct = default);
    Task ShowAsync(string title, string message, string? closeText = null, CancellationToken ct = default);
    Task<string?> ShowInputAsync(string title, string message, string? placeholder = null, string? defaultValue = null, CancellationToken ct = default);
}
