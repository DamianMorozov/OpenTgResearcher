// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Helpers;

public static class TgFrameExtensions
{
	public static object? GetPageViewModel(this Frame frame)
	{
		var content = frame.Content;
		var type = content?.GetType();
		var properties = type?.GetProperties();
		if (properties != null)
			foreach (var property in properties)
			{
				if (property.Name.Contains("ViewModel"))
					return property.GetValue(frame.Content, null);
			}

		//var property = type?.GetProperty("ViewModel");
		//return property?.GetValue(frame.Content, null);
		return null;
	}
}
