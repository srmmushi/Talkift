using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

namespace Talkift.Client.Helpers;

public static class DispatcherExtensions
{
    public static async Task RunOnUIAsync(this DispatcherQueue dispatcherQueue, Action action, CancellationToken ct = default)
    {
        if (dispatcherQueue == null)
        {
            action();
            return;
        }

        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var reg = ct.Register(() => tcs.TrySetCanceled(ct));

        dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                action();
                tcs.TrySetResult(null);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        await tcs.Task;
        reg.Dispose();
    }

    public static async Task<T> RunOnUIAsync<T>(this DispatcherQueue dispatcherQueue, Func<T> func, CancellationToken ct = default)
    {
        if (dispatcherQueue == null)
            return func();

        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        var reg = ct.Register(() => tcs.TrySetCanceled(ct));

        dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                tcs.TrySetResult(func());
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        var result = await tcs.Task;
        reg.Dispose();
        return result;
    }

    public static T RunOnUI<T>(this DispatcherQueue dispatcherQueue, Func<T> func)
    {
        if (dispatcherQueue == null)
            return func();

        T result = default!;
        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                result = func();
                tcs.TrySetResult(null);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        tcs.Task.Wait();
        return result;
    }

    public static void RunOnUI(this DispatcherQueue dispatcherQueue, Action action)
    {
        if (dispatcherQueue == null)
        {
            action();
            return;
        }

        dispatcherQueue.TryEnqueue(() => action());
    }
}
