using System;
using System.Threading;
using System.Threading.Tasks;

namespace Talkift.Client.Services;

public enum ThemePreference
{
    Light,
    Dark,
    System
}

public interface IThemeService : IDisposable
{
    ThemePreference CurrentPreference { get; }
    bool IsDarkMode { get; }

    event EventHandler<bool>? ThemeChanged;

    Task SetThemeAsync(ThemePreference preference, CancellationToken ct = default);
    Task<ThemePreference> LoadThemeAsync(CancellationToken ct = default);
    Task SaveThemeAsync(ThemePreference preference, CancellationToken ct = default);
}
