namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Frame extensions </summary>
public static class TgFrameExtensions
{
    /// <summary> Get the ViewModel from the Frame's content </summary>
	public static object? GetPageViewModel(this Frame frame)
    {
        var dispatcher = App.MainWindow?.DispatcherQueue;
        if (dispatcher is null) return null;

        // If we are already on the UI thread, execute directly
        if (dispatcher.HasThreadAccess)
        {
            return ExtractViewModel(frame);
        }

        // Otherwise, wait for execution on the UI thread
        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        dispatcher.TryEnqueue(() =>
        {
            try
            {
                tcs.TrySetResult(ExtractViewModel(frame));
            }
            catch (Exception ex)
            {
                TgLogUtils.WriteException(ex, $"Error executing {nameof(GetPageViewModel)}");
                tcs.TrySetResult(null);
            }
        });

        return tcs.Task.GetAwaiter().GetResult();
    }

    public static async Task<object?> GetPageViewModelAsync(this Frame frame)
    {
        var dispatcher = App.MainWindow?.DispatcherQueue;
        if (dispatcher is null) return null;

        if (dispatcher.HasThreadAccess)
            return ExtractViewModel(frame);

        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        dispatcher.TryEnqueue(() =>
        {
            try { tcs.TrySetResult(ExtractViewModel(frame)); }
            catch (Exception ex)
            {
                TgLogUtils.WriteException(ex, $"Error executing {nameof(GetPageViewModelAsync)}");
                tcs.TrySetResult(null);
            }
        });
        return await tcs.Task.ConfigureAwait(false);
    }

    private static object? ExtractViewModel(Frame frame)
    {
        var content = frame.Content;
        var type = content?.GetType();
        var property = type?
            .GetProperties()
            .FirstOrDefault(p => p.Name.Contains("ViewModel", StringComparison.OrdinalIgnoreCase));
        return property?.GetValue(content, null);
    }
}
