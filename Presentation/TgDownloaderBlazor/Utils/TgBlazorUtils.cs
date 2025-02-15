// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderBlazor.Utils;

public static class TgBlazorUtils
{
	#region Public and private methods

	public static async Task RunFuncAsync(Func<Task> action)
	{
		await Task.Delay(1);

		try
		{
			await action();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
		}
	}

	public static async Task RunFuncAsync(Func<Task> action, Action<string> actionException, Action actionFinally)
	{
		await Task.Delay(1);

		try
		{
			await action();
		}
		catch (Exception ex)
		{
#if DEBUG
			Debug.WriteLine(ex, TgConstants.LogTypeSystem);
			Console.WriteLine(ex);
#endif
			if (ex.InnerException is null)
				actionException(ex.Message);
			else
				actionException(ex.Message + Environment.NewLine + ex.InnerException.Message);
		}
		finally
		{
			actionFinally();
		}
	}

	public static async Task RunActionAsync(Action action)
	{
		await Task.Delay(1);

		try
		{
			action();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
		}
	}

	public static async Task RunActionAsync(Action action, Action actionFinally)
	{
		await Task.Delay(1);

		try
		{
			action();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
		}
		finally
		{
			actionFinally();
		}
	}

	#endregion
}