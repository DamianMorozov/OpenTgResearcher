// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Core.Services;

public class FileService : IFileService
{
	public T Read<T>(string folderPath, string fileName)
	{
		var path = Path.Combine(folderPath, fileName);
		if (File.Exists(path))
		{
			var json = File.ReadAllText(path);
			return JsonSerializer.Deserialize<T>(json, TgJsonSerializerUtils.GetJsonOptions());
		}

		return default;
	}

	public void Save<T>(string folderPath, string fileName, T content)
	{
		if (!Directory.Exists(folderPath))
		{
			Directory.CreateDirectory(folderPath);
		}

		var fileContent = JsonSerializer.Serialize(content);
		File.WriteAllText(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
	}

	public void Delete(string folderPath, string fileName)
	{
		if (fileName != null && File.Exists(Path.Combine(folderPath, fileName)))
		{
			File.Delete(Path.Combine(folderPath, fileName));
		}
	}
}
