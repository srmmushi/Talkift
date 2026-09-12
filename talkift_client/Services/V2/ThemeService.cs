using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Talkift.Client.Services.V2;

public sealed class ThemeService : IThemeService
{
    private ThemePreference _preference = ThemePreference.System;
    private bool _isDarkMode;
    private bool _disposed;

    public ThemePreference CurrentPreference => _preference;
    public bool IsDarkMode => _isDarkMode;

    public event EventHandler<bool>? ThemeChanged;

    public Task SetThemeAsync(ThemePreference preference, CancellationToken ct = default)
    {
        _preference = preference;
        var isDark = preference switch
        {
            ThemePreference.Dark => true,
            ThemePreference.Light => false,
            _ => IsSystemDark()
        };

        _isDarkMode = isDark;

        if (App.CurrentWindow?.Content is Microsoft.UI.Xaml.FrameworkElement root)
        {
            root.RequestedTheme = isDark
                ? Microsoft.UI.Xaml.ElementTheme.Dark
                : Microsoft.UI.Xaml.ElementTheme.Light;
        }

        ThemeChanged?.Invoke(this, isDark);
        return Task.CompletedTask;
    }

    public async Task<ThemePreference> LoadThemeAsync(CancellationToken ct = default)
    {
        try
        {
            var path = System.IO.Path.Combine(StorageService.DataDir, "theme.json");
            if (System.IO.File.Exists(path))
            {
                var json = await System.IO.File.ReadAllTextAsync(path, ct);
                var doc = JsonSerializer.Deserialize<JsonElement>(json);
                var pref = doc.TryGetProperty("preference", out var p) ? p.GetString() ?? "System" : "System";
                _preference = Enum.TryParse<ThemePreference>(pref, true, out var tp) ? tp : ThemePreference.System;
            }
        }
        catch
        {
            _preference = ThemePreference.System;
        }

        await SetThemeAsync(_preference, ct);
        return _preference;
    }

    public async Task SaveThemeAsync(ThemePreference preference, CancellationToken ct = default)
    {
        var path = System.IO.Path.Combine(StorageService.DataDir, "theme.json");
        var dir = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            System.IO.Directory.CreateDirectory(dir);

        var data = new { preference = preference.ToString() };
        var json = JsonSerializer.Serialize(data);
        await System.IO.File.WriteAllTextAsync(path, json, ct);
        await SetThemeAsync(preference, ct);
    }

    private static bool IsSystemDark()
    {
        try
        {
            var settings = new Windows.UI.ViewManagement.UISettings();
            return settings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).R < 128;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
