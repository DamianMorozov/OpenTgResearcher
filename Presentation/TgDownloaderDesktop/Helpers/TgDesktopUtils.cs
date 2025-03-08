// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Helpers;

/// <summary> Desktop utils </summary>
public static class TgDesktopUtils
{
	#region Public and private methods

	//public static async Task<bool> CheckFileStorageExistsAsync(string fullPath)
	//{
	//	var result = await CheckFileStorageExistsCoreAsync(fullPath);
	//	if (result)
	//		return true;
	//	return await CheckFileStorageExistsCoreAsync(fullPath);
	//}

	//private static async Task<bool> CheckFileStorageExistsCoreAsync(string fullPath)
	//{
 //       try
 //       {
	//	    var folder = Path.GetDirectoryName(fullPath) ?? string.Empty;
	//	    if (string.IsNullOrEmpty(folder))
	//		    return false;
	//	    var fileName = Path.GetFileName(fullPath);
	//		var storageFolder = await StorageFolder.GetFolderFromPathAsync(folder);
 //           if (storageFolder is null)
 //               return false;
 //           var storageFile = await storageFolder.GetFileAsync(fileName);
 //           return storageFile.IsAvailable;
 //       }
 //       catch (Exception ex)
 //       {
	//		LogFatal(ex);
 //       }
 //       return false;
	//}

	public static async Task<bool> DeleteFileStorageExistsAsync(string fullPath)
	{
		var result = await DeleteFileStorageExistsCoreAsync(fullPath);
		if (result)
			return true;
		return await DeleteFileStorageExistsCoreAsync(fullPath);
	}

	public static async Task<bool> DeleteFileStorageExistsCoreAsync(string fullPath)
	{
		var folder = string.Empty;
		var fileName = string.Empty;
		try
		{
			folder = Path.GetDirectoryName(fullPath) ?? string.Empty;
			if (string.IsNullOrEmpty(folder))
				return false;
			fileName  = Path.GetFileName(fullPath);
			var storageFolder = await StorageFolder.GetFolderFromPathAsync(folder);
			if (storageFolder is null)
				return false;
			var storageFile = await storageFolder.GetFileAsync(fileName);
			if (storageFile.IsAvailable)
				await storageFile.DeleteAsync();
		}
		catch (Exception ex)
		{
			TgLogUtils.LogFatal(ex, $"{Path.Combine(folder, fileName)}");
		}
		return false;
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
			TgLogUtils.LogFatal(ex);
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

	//public static async void OnClipboardWriteClick(this TgEfMessageDto dto, object sender, RoutedEventArgs e)
	//{
	//	if (sender is Button button)
	//	{
	//		var address = button.Tag?.ToString();
	//		if (string.IsNullOrEmpty(address))
	//			return;
	//		if (!string.IsNullOrEmpty(address))
	//		{
	//			var dataPackage = new DataPackage();
	//			dataPackage.SetText(address);
	//			Clipboard.SetContent(dataPackage);
	//			await ContentDialogAsync(TgResourceExtensions.GetClipboard(), address);
	//		}
	//	}
	//}

	#endregion
}