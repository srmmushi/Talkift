using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Talkift.Client.Helpers;

public static class RelayCommandExtensions
{
    public static RelayCommand<T?> ToRelayCommand<T>(this Action<T?> action) where T : class
    {
        return new RelayCommand<T?>(action ?? throw new ArgumentNullException(nameof(action)));
    }

    public static AsyncRelayCommand<T?> ToAsyncRelayCommand<T>(this Func<T?, Task> asyncAction) where T : class
    {
        return new AsyncRelayCommand<T?>(asyncAction ?? throw new ArgumentNullException(nameof(asyncAction)));
    }

    public static RelayCommand ToRelayCommand(this Action action)
    {
        return new RelayCommand(action ?? throw new ArgumentNullException(nameof(action)));
    }

    public static AsyncRelayCommand ToAsyncRelayCommand(this Func<Task> asyncAction)
    {
        return new AsyncRelayCommand(asyncAction ?? throw new ArgumentNullException(nameof(asyncAction)));
    }

    public static RelayCommand<string> ToRelayCommandString(this Action<string> action)
    {
        return new RelayCommand<string>(action ?? throw new ArgumentNullException(nameof(action)));
    }
}
