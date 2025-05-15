// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using CommunityToolkit.WinUI;

namespace OpenTgResearcherDesktop.Helpers;

internal static class TgDispatcherQueueExtensions
{
	public static void TryEnqueueWithLog(this Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue, Action action, string method = "",
		[CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
	{
		dispatcherQueue.TryEnqueue(() =>
		{
			try
			{
				action();
				if (!string.IsNullOrEmpty(method))
					TgLogUtils.LogInformationProxy($"{method} complete.", filePath, lineNumber, memberName);
			}
			catch (Exception ex)
			{
				TgLogUtils.LogFatalProxy(ex, method, filePath, lineNumber, memberName);
				throw;
			}
			finally
			{
				if (!string.IsNullOrEmpty(method))
					TgLogUtils.LogInformationProxy($"{method} finally.", filePath, lineNumber, memberName);
			}
		});
	}

	public static void TryEnqueueWithLog(this Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue, Microsoft.UI.Dispatching.DispatcherQueuePriority priority, 
		Action action, string method = "",
		[CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
	{
		dispatcherQueue.TryEnqueue(priority, () =>
		{
			try
			{
				action();
				if (!string.IsNullOrEmpty(method))
					TgLogUtils.LogInformationProxy($"{method} complete.", filePath, lineNumber, memberName);
			}
			catch (Exception ex)
			{
				TgLogUtils.LogFatalProxy(ex, method, filePath, lineNumber, memberName);
				throw;
			}
			finally
			{
				if (!string.IsNullOrEmpty(method))
					TgLogUtils.LogInformationProxy($"{method} finally.", filePath, lineNumber, memberName);
			}
		});
	}

	public static Task TryEnqueueWithLogAsync(this Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue, Func<Task> func, string method = "",
		[CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
	{
		var tcs = new TaskCompletionSource<object?>();
		dispatcherQueue.EnqueueAsync(async () =>
		{
			try
			{
				await func();
				tcs.SetResult(null);
				if (!string.IsNullOrEmpty(method))
					TgLogUtils.LogInformationProxy($"{method} complete.", filePath, lineNumber, memberName);
			}
			catch (Exception ex)
			{
				TgLogUtils.LogFatalProxy(ex, method, filePath, lineNumber, memberName);
				tcs.SetException(ex);
			}
			finally
			{
				if (!string.IsNullOrEmpty(method))
					TgLogUtils.LogInformationProxy($"{method} finally.", filePath, lineNumber, memberName);
			}
		});
		return tcs.Task;
	}

	public static async Task TryFuncWithLogAsync(Func<Task> func, string method = "",
		[CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
	{
		var tcs = new TaskCompletionSource<object?>();
		try
		{
			await func();
			tcs.SetResult(null);
			if (!string.IsNullOrEmpty(method))
				TgLogUtils.LogInformationProxy($"{method} complete.", filePath, lineNumber, memberName);
		}
		catch (Exception ex)
		{
			TgLogUtils.LogFatalProxy(ex, method, filePath, lineNumber, memberName);
			tcs.SetException(ex);
		}
		finally
		{
			if (!string.IsNullOrEmpty(method))
				TgLogUtils.LogInformationProxy($"{method} finally.", filePath, lineNumber, memberName);
		}
	}
}
