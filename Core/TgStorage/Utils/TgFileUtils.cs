﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Utils;

/// <summary>
/// Files utilities.
/// </summary>
public static class TgFileUtils
{
	#region Public and private fields, properties, constructor

	private static TgLocaleHelper TgLocale => TgLocaleHelper.Instance;
	private static TgLogHelper TgLog => TgLogHelper.Instance;
	public static string FileAppXmlSettings => $"{AppDomain.CurrentDomain.BaseDirectory}TgDownloader.xml";
	public static string FileTgSession => "TgDownloader.session";
	public static string FileEfStorage => "TgStorage.db";
	//public static string FileDeprecatedStorage => "TgDownloader.db";

	#endregion

	#region Public and private methods

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
		//Ensure.NotNull(stream, nameof(stream));

		char[] byteBuffer = new char[1024 * 1024];
		const int bytesAtTheTime = 4;
		char? detectedEol = null;
		char? currentChar = null;

		int bytesRead;
		while ((bytesRead = streamReader.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
		{
			int i = 0;
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
		streamReader.Dispose();
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
			TgLog.MarkupLine($"{TgLocale.StatusException}: " + TgLog.GetMarkupString(ex.Message));
			if (ex.InnerException is not null)
				TgLog.MarkupLine($"{TgLocale.StatusInnerException}: " + TgLog.GetMarkupString(ex.InnerException.Message));
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
        string os = Environment.OSVersion.Platform.ToString();
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

    #endregion
}