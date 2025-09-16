namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Desktop utils </summary>
public static class TgDesktopUtils
{
    #region Methods

    /// <summary> Invoke task on UI thread if dispatcher queue is available </summary>
    public static async Task InvokeOnUIThreadAsync(Func<Task> task)
    {
        if (App.MainWindow?.DispatcherQueue is not null)
        {
            var tcs = new TaskCompletionSource();
            var enqueued = App.MainWindow?.DispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    await task().ConfigureAwait(false);
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    TgLogUtils.WriteException(ex, $"Error executing {nameof(InvokeOnUIThreadAsync)} task");
                    tcs.SetException(ex);
                }
            });
            if (enqueued == true)
            {
                await tcs.Task;
                return;
            }
        }

        try
        {
            await task().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, $"Error executing {nameof(InvokeOnUIThreadAsync)} task");
        }
    }

    /// <summary> Invoke task on UI thread if dispatcher queue is available </summary>
    public static void InvokeOnUIThread(Action action)
    {
        if (App.MainWindow?.DispatcherQueue is not null)
        {
            var enqueued = App.MainWindow?.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    TgLogUtils.WriteException(ex, $"Error executing {nameof(InvokeOnUIThreadAsync)} task");
                }
            });
            if (enqueued == true)
            {
                return;
            }
        }

        try
        {
            action();
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, $"Error executing {nameof(InvokeOnUIThreadAsync)} task");
        }
    }

    public static async Task DeleteFileAsync(string fullPath)
	{
		var folder = string.Empty;
		var fileName = string.Empty;
		try
		{
			folder = Path.GetDirectoryName(fullPath) ?? string.Empty;
			if (string.IsNullOrEmpty(folder))
				return;
			fileName  = Path.GetFileName(fullPath);
			var storageFolder = await StorageFolder.GetFolderFromPathAsync(folder);
			if (storageFolder is null)
				return;
			var storageFile = await storageFolder.GetFileAsync(fileName);
			if (storageFile.IsAvailable)
				await storageFile.DeleteAsync();
		}
		catch (Exception ex)
		{
			TgLogUtils.WriteExceptionWithMessage(ex, $"{Path.Combine(folder, fileName)}");
		}
	}

	public static async Task<long> CalculateDirSizeAsync(string folderPath)
	{
		long totalSize = 0;
		if (string.IsNullOrEmpty(folderPath)) return 0;
		try
		{
			var storageFolder = await StorageFolder.GetFolderFromPathAsync(folderPath);
			if (storageFolder is null) return 0;
			// Get all files and subdirectories
			var files = await storageFolder.GetFilesAsync();
			var subFolders = await storageFolder.GetFoldersAsync();
			// Calculate the size of all files in the current directory
			foreach (var file in files)
			{
				//totalSize += file.Size;
				var properties = await file.GetBasicPropertiesAsync();
				totalSize += (long)properties.Size;
			}
			// Recursively count the size in subdirectories
			foreach (var subFolder in subFolders)
			{
				totalSize += await CalculateDirSizeAsync(subFolder.Path);
			}
		}
		catch (Exception ex)
		{
			TgLogUtils.WriteException(ex);
			return 0;
		}
		return totalSize;
	}

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

    /// <summary> Verify paid license </summary>
    public static bool VerifyPaidLicense() => App.BusinessLogicManager.LicenseService.CurrentLicense?.CheckPaidLicense() ?? false;

    /// <summary> Get license type </summary>
    public static TgEnumLicenseType GetLicenseType() => App.BusinessLogicManager.LicenseService.CurrentLicense?.LicenseType ?? TgEnumLicenseType.No;

    /// <summary> Get resource brush </summary>
    public static Brush GetResourceBrush(string resourceKey, Brush? fallback = null)
    {
        if (Application.Current.Resources.TryGetValue(resourceKey, out var brush) && brush is Brush result)
        {
            return result;
        }
        return fallback ?? new SolidColorBrush(Colors.Transparent);
    }

    #endregion
}
