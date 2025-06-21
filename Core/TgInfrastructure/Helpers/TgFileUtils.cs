// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Helpers;

/// <summary> File utils </summary>
public static class TgFileUtils
{
    #region Public and private fields, properties, constructor

    private static TgLogHelper TgLog => TgLogHelper.Instance;
	public static string BaseDirectory = string.Empty;
	public static string FileAppXmlSettings => !string.IsNullOrEmpty(BaseDirectory) ? Path.Combine(BaseDirectory, "OpenTgResearcher.xml") : string.Empty;
	public static string FileTgSession => "OpenTgResearcher.session";
	public static string LogsDirectory = "Logs";

	static TgFileUtils()
	{
		try
		{
			BaseDirectory = AppContext.BaseDirectory;
		}
		catch (Exception)
		{
			//
		}
	}

    #endregion

    #region Public and private methods

    public static string GetShortFilePath(string filePath) => string.IsNullOrEmpty(filePath) ? string.Empty : Path.GetFileName(filePath);
    
    public static ulong GetContentRowsCountSlow(string sourceFile)
	{
		ulong rows = 0;
		using StreamReader streamReader = new(sourceFile);
		while (streamReader.ReadLine() is not null)
		{
			rows++;
		}
		streamReader.Close();
		return rows;
	}

	public static ulong GetContentRowsCountFast(string sourceFile)
	{
		ulong lineCount = 0L;
		using StreamReader streamReader = new(sourceFile);

		var byteBuffer = new char[1024 * 1024];
		const int bytesAtTheTime = 4;
		char? detectedEol = null;
		char? currentChar = null;

		int bytesRead;
		while ((bytesRead = streamReader.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
		{
			var i = 0;
			for (; i <= bytesRead - bytesAtTheTime; i += bytesAtTheTime)
			{
				currentChar = byteBuffer[i];

				if (detectedEol is not null)
				{
					if (currentChar == detectedEol)
					{ lineCount++; }

					currentChar = byteBuffer[i + 1];
					if (currentChar == detectedEol)
					{ lineCount++; }

					currentChar = byteBuffer[i + 2];
					if (currentChar == detectedEol)
					{ lineCount++; }

					currentChar = byteBuffer[i + 3];
					if (currentChar == detectedEol)
					{ lineCount++; }
				}
				else
				{
					if (currentChar is '\n' or '\r')
					{
						detectedEol = currentChar;
						lineCount++;
					}
					i -= bytesAtTheTime - 1;
				}
			}

			for (; i < bytesRead; i++)
			{
				currentChar = byteBuffer[i];

				if (detectedEol is not null)
				{
					if (currentChar == detectedEol)
					{ lineCount++; }
				}
				else
				{
					if (currentChar is '\n' or '\r')
					{
						detectedEol = currentChar;
						lineCount++;
					}
				}
			}
		}

		if (currentChar is not '\n' && currentChar is not '\r' && currentChar is not null)
		{
			lineCount++;
		}

		streamReader.Close();
		return lineCount;
	}

	public static long CalculateDirSize(string dir)
	{
		if (!Directory.Exists(dir))
			return 0L;
		try
		{
			return new FileSystemEnumerable<long>(dir,
				(ref FileSystemEntry entry) => entry.Length,
				new()
				{
					RecurseSubdirectories = true,
					AttributesToSkip = FileAttributes.ReparsePoint, // Avoiding infinite loop
					IgnoreInaccessible = true,
					MatchCasing = MatchCasing.PlatformDefault,
					ReturnSpecialDirectories = false,
					//MatchType = MatchType.Win32,
					//BufferSize = 0,
					//MaxRecursionDepth = 0,
				})
			{ ShouldIncludePredicate = (ref FileSystemEntry entry) => !entry.IsDirectory }.Sum();
		}
		catch (Exception ex)
		{
			TgLog.MarkupLine($"  Status exception: " + TgLog.GetMarkupString(ex.Message));
			if (ex.InnerException is not null)
				TgLog.MarkupLine($"  Status inner exception: " + TgLog.GetMarkupString(ex.InnerException.Message));
			return 0L;
		}
	}

	public static long CalculateFileSize(string file) =>
		!File.Exists(file) ? 0L : new FileInfo(file).Length;

	public static string GetFileSizeString(long value) =>
		value > 0
			? value switch
			{
				< 1024 => $"{value:###} B",
				< 1024 * 1024 => $"{(double)value / 1024L:###} KB",
				< 1024 * 1024 * 1024 => $"{(double)value / 1024L / 1024L:###} MB",
				_ => $"{(double)value / 1024L / 1024L / 1024L:###} GB"
			}
			: "0 B";

	public static string GetDefaultDirectory()
	{
		var os = Environment.OSVersion.Platform.ToString();
		// Windows
		if (os == "Win32NT" || os == "Win32S" || os == "Win32Windows" || os == "WinCE")
			return "C:";
		// Linux or Mac OS
		if (os == "Unix" || os == "X11")
			return "/home/username";
		// Android
		if (os.Contains("Android"))
			return "/sdcard";
		// WebAssembly
		if (os.Contains("WebAssembly"))
			return "/";
		// Other.
		return Environment.CurrentDirectory;
	}

    /// <summary> Normalizes a file path by replacing backslashes with forward slashes, removing duplicate slashes, 
    /// and ensuring it starts with a slash if it's an absolute path </summary>
    public static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;

        // Gat absolute path if possible
        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path);
        }
        catch
        {
            fullPath = path;
        }

        // Replace backslashes with forward slashes
        fullPath = fullPath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

        // Remove duplicate slashes
        var pattern = $"{Regex.Escape(Path.DirectorySeparatorChar.ToString())}{{2,}}";
        fullPath = Regex.Replace(fullPath, pattern, Path.DirectorySeparatorChar.ToString());

        return fullPath;
    }

    #endregion
}
