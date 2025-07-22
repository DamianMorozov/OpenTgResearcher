// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Fram extensions </summary>
public static class TgFrameExtensions
{
    /// <summary> Get the ViewModel from the Frame's content </summary>
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
		return null;
	}
}
