using System.Collections.Concurrent;
using UnityEngine;

namespace FootballManager.Plugin;

// Unity and game objects may only be touched on the main thread: requests queue their work here and Update() runs it.
// Injected into IL2CPP, so it stays public with the IntPtr constructor.
public sealed class MainThreadDispatcher : MonoBehaviour
{
    private static readonly ConcurrentQueue<Action> Work = new();
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// The current request's expiry, which flows into the work it queues: once the request has been answered with a
    /// timeout, work it queued and that hasn't started is dropped, so a client retrying doesn't get both runs.
    /// </summary>
    public static readonly AsyncLocal<CancellationToken> RequestExpiry = new();

    public MainThreadDispatcher(IntPtr pointer) : base(pointer)
    {
    }

    public static Task<T> RunAsync<T>(Func<T> work)
    {
        return Enqueue(work, RequestExpiry.Value);
    }

    /// <summary>For tearing down what earlier work set up: runs even once the request has expired.</summary>
    public static Task<T> RunTeardownAsync<T>(Func<T> work)
    {
        return Enqueue(work, CancellationToken.None);
    }

    private static Task<T> Enqueue<T>(Func<T> work, CancellationToken expiry)
    {
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        Work.Enqueue(() =>
        {
            if (expiry.IsCancellationRequested)
            {
                completion.SetCanceled(expiry);
                return;
            }

            try
            {
                completion.SetResult(work());
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        });

        return completion.Task;
    }

    // For work the game answers over several frames: start it, check it between frames until done or out of time,
    // then finish it, which also runs on timeout so whatever start set up gets torn down.
    public static async Task<TResult> RunAcrossFramesAsync<TState, TResult>(
        Func<TState> start, Func<TState, bool> isDone, Func<TState, TResult> finish, TimeSpan timeout)
    {
        var state = await RunAsync(start);
        var deadline = DateTime.UtcNow + timeout;

        try
        {
            while (DateTime.UtcNow < deadline && !await RunAsync(() => isDone(state)))
            {
                await Task.Delay(PollInterval);
            }
        }
        catch
        {
            // Still tear down; the polling error is the one worth reporting.
            try
            {
                await RunTeardownAsync(() => finish(state));
            }
            catch (Exception)
            {
            }

            throw;
        }

        return await RunTeardownAsync(() => finish(state));
    }

    public void Update()
    {
        while (Work.TryDequeue(out var action))
        {
            action();
        }
    }
}
