namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Desktop utils </summary>
public static class TgDesktopUtils
{
    #region Methods

    /// <summary> Invoke task on UI thread if dispatcher queue is available </summary>
    public static async Task InvokeOnUIThreadAsync(Func<Task> task, CancellationToken ct = default)
    {
        var dispatcher = App.MainWindow?.DispatcherQueue;

        // If we are already on the UI thread, execute directly
        if (dispatcher?.HasThreadAccess == true)
        {
            if (ct.IsCancellationRequested) return;
            try { await task().ConfigureAwait(false); }
            catch (OperationCanceledException) { /* Silent */ }
            return;
        }

        // No dispatcher — execute on the current thread
        if (dispatcher is null)
        {
            if (ct.IsCancellationRequested) return;
            try { await task().ConfigureAwait(false); }
            catch (OperationCanceledException) { /* silent */ }
            catch (Exception ex) { TgLogUtils.WriteException(ex, $"Error executing {nameof(InvokeOnUIThreadAsync)} task"); }
            return;
        }

        // Enqueue on UI with wait for completion without throwing TaskCanceledException
        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        var enqueued = dispatcher.TryEnqueue(async () =>
        {
            try
            {
                if (ct.IsCancellationRequested) { tcs.TrySetResult(null); return; }
                await task().ConfigureAwait(false);
                tcs.TrySetResult(null);
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetResult(null); // silent
            }
            catch (Exception ex)
            {
                TgLogUtils.WriteException(ex, $"Error executing {nameof(InvokeOnUIThreadAsync)} task");
                tcs.TrySetException(ex);
            }
        });

        if (enqueued == true)
        {
            await tcs.Task.ConfigureAwait(false);
        }
        else
        {
            // Fallback — dispatcher declined: execute in the current thread
            if (ct.IsCancellationRequested) return;
            try { await task().ConfigureAwait(false); }
            catch (OperationCanceledException) { /* silent */ }
            catch (Exception ex) { TgLogUtils.WriteException(ex, $"Error executing {nameof(InvokeOnUIThreadAsync)} task"); }
        }
    }

    /// <summary> Invoke task on UI thread if dispatcher queue is available </summary>
    public static void InvokeOnUIThread(Action action)
    {
        var dispatcher = App.MainWindow?.DispatcherQueue;

        // Fast path
        if (dispatcher?.HasThreadAccess == true)
        {
            try { action(); }
            catch (Exception ex) { TgLogUtils.WriteException(ex, $"Error executing {nameof(InvokeOnUIThread)} action"); }
            return;
        }

        if (dispatcher is null)
        {
            try { action(); }
            catch (Exception ex) { TgLogUtils.WriteException(ex, $"Error executing {nameof(InvokeOnUIThread)} action"); }
            return;
        }

        var done = new ManualResetEventSlim(false);
        Exception? error = null;

        dispatcher.TryEnqueue(() =>
        {
            try { action(); }
            catch (Exception ex) { error = ex; }
            finally { done.Set(); }
        });

        // WARNING: only safe from the background thread; on the UI, this will cause a deadlock
        if (!dispatcher.HasThreadAccess)
        {
            done.Wait();
            if (error is not null)
                TgLogUtils.WriteException(error, $"Error executing {nameof(InvokeOnUIThread)} action");
        }
    }

    /// <summary> Delete file by full path </summary>
    public static async Task DeleteFileAsync(string fullPath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fullPath)) return;

        var folder = string.Empty;
        var fileName = string.Empty;

        try
        {
            folder = Path.GetDirectoryName(fullPath) ?? string.Empty;
            if (string.IsNullOrEmpty(folder)) return;

            fileName = Path.GetFileName(fullPath);
            if (ct.IsCancellationRequested) return;

            StorageFolder? storageFolder = null;
            try
            {
                storageFolder = await StorageFolder.GetFolderFromPathAsync(folder).AsTask(ct).ConfigureAwait(false);
            }
            catch (FileNotFoundException) { return; }
            catch (UnauthorizedAccessException) { return; }

            if (storageFolder is null) return;

            StorageFile? storageFile = null;
            try
            {
                storageFile = await storageFolder.GetFileAsync(fileName).AsTask(ct).ConfigureAwait(false);
            }
            catch (FileNotFoundException) { return; }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
            {
                TgLogUtils.WriteExceptionWithMessage(ex, $"{Path.Combine(folder, fileName)}");
                return;
            }

            if (storageFile is not null && storageFile.IsAvailable)
            {
                await storageFile.DeleteAsync().AsTask(ct).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // silent
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteExceptionWithMessage(ex, $"{Path.Combine(folder, fileName)}");
        }
    }

    public static Task<long> CalculateDirSizeAsync(string folderPath, CancellationToken ct = default) =>
        Task.Run(async () =>
        {
            if (string.IsNullOrWhiteSpace(folderPath)) return 0L;
            long totalSize = 0;

            try
            {
                StorageFolder? root = null;
                try
                {
                    root = await StorageFolder.GetFolderFromPathAsync(folderPath).AsTask(ct).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is UnauthorizedAccessException)
                {
                    return 0L;
                }

                if (root is null) return 0L;

                var folders = new Stack<StorageFolder>();
                folders.Push(root);

                while (folders.Count > 0)
                {
                    if (ct.IsCancellationRequested) return 0L;

                    var current = folders.Pop();

                    IReadOnlyList<StorageFile> files;
                    IReadOnlyList<StorageFolder> subFolders;
                    try
                    {
                        files = await current.GetFilesAsync().AsTask(ct).ConfigureAwait(false);
                        subFolders = await current.GetFoldersAsync().AsTask(ct).ConfigureAwait(false);
                    }
                    catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
                    {
                        TgLogUtils.WriteException(ex);
                        continue;
                    }

                    foreach (var f in files)
                    {
                        if (ct.IsCancellationRequested) return 0L;
                        try
                        {
                            var prop = await f.GetBasicPropertiesAsync().AsTask(ct).ConfigureAwait(false);
                            totalSize += (long)prop.Size;
                        }
                        catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
                        {
                            TgLogUtils.WriteException(ex);
                        }
                    }

                    foreach (var sf in subFolders)
                        folders.Push(sf);
                }
            }
            catch (OperationCanceledException) { /* cancel */ }
            catch (Exception ex)
            {
                TgLogUtils.WriteException(ex);
                return 0L;
            }

            return totalSize;
        }, CancellationToken.None);

    public static string ExtractUrl(string rawUrl)
    {
        if (rawUrl.StartsWith("ms-resource:///Files/"))
        {
            var encodedUrl = rawUrl.Substring("ms-resource:///Files/".Length);
            var decodedUrl = Uri.UnescapeDataString(encodedUrl);
            return decodedUrl.Replace("\"", "");
        }
        return rawUrl;
    }

    public static T? FindName<T>(this DependencyObject parent, string name) where T : FrameworkElement
    {
        if (parent == null) return null;
        if (parent is T element && element.Name == name)
        {
            return element;
        }
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            var result = FindName<T>(child, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    /// <summary> Verify license </summary>
    public static bool VerifyLicense() => App.BusinessLogicManager.LicenseService.CurrentLicense?.CheckLicense() ?? false;

    /// <summary> Get license type </summary>
    public static TgEnumLicenseType GetLicenseType() => App.BusinessLogicManager.LicenseService.CurrentLicense?.LicenseType ?? TgEnumLicenseType.No;

    /// <summary> Get resource brush </summary>
    public static Brush GetResourceBrush(string resourceKey, Brush? fallback = null)
    {
        try
        {
            // It is preferable to access Resources on the UI thread.
            var dispatcher = App.MainWindow?.DispatcherQueue;
            if (dispatcher?.HasThreadAccess == true)
            {
                if (Application.Current.Resources.TryGetValue(resourceKey, out var brush) && brush is Brush result)
                    return result;
                return fallback ?? new SolidColorBrush(Colors.Transparent);
            }

            // If not on UI: get the value synchronously via InvokeOnUIThread
            Brush resultBrush = fallback ?? new SolidColorBrush(Colors.Transparent);
            InvokeOnUIThread(() =>
            {
                if (Application.Current.Resources.TryGetValue(resourceKey, out var brush) && brush is Brush result)
                    resultBrush = result;
            });
            return resultBrush;
        }
        catch
        {
            return fallback ?? new SolidColorBrush(Colors.Transparent);
        }
    }

    #endregion
}
