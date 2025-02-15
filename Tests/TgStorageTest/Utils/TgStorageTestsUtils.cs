// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
#pragma warning disable NUnit1033

namespace TgStorageTest.Utils;

[TestFixture]
internal sealed class TgStorageTestsUtils : TgDbContextTestsBase
{
	#region Public and private methods

	[Test]
	public void Check_version_last_async()
	{
		Assert.DoesNotThrowAsync(async () =>
		{
			TgEfVersionRepository versionRepository = new();
			await versionRepository.FillTableVersionsAsync();

			var versionLast = await versionRepository.GetLastVersionAsync();

			Assert.That(versionLast.Version == versionRepository.LastVersion);
			TestContext.WriteLine($"{nameof(versionLast)}.{nameof(versionLast.Version)}: {versionLast.Version}");
			var versions = (await versionRepository.GetListAsync(TgEnumTableTopRecords.All, 0))
				.Items.ToList();
			TestContext.WriteLine($"Found {versions.Count()} items");
		});
	}

	[Test]
	public void Check_version_last_with_add_default_async()
	{
		Assert.DoesNotThrowAsync(async () =>
		{
			TgEfVersionRepository versionRepository = new();
			await versionRepository.FillTableVersionsAsync();

			var version = new TgEfVersionEntity();
			await versionRepository.SaveAsync(version);
			var versionLast = await versionRepository.GetLastVersionAsync();

			Assert.That(versionLast.Version == versionRepository.LastVersion);
			TestContext.WriteLine($"{nameof(versionLast)}.{nameof(versionLast.Version)}: {versionLast.Version}");
			var versions = (await versionRepository.GetListAsync(TgEnumTableTopRecords.All, 0))
				.Items.ToList();
			TestContext.WriteLine($"Found {versions.Count} items");
		});
	}

	[Test]
	public void Check_version_last_when_only_default_async()
	{
		Assert.DoesNotThrowAsync(async () =>
		{
			TgEfVersionRepository versionRepository = new();
			await versionRepository.FillTableVersionsAsync();

			await versionRepository.DeleteAllAsync();
			var version = new TgEfVersionEntity();
			await versionRepository.SaveAsync(version);
			var versionLast = await versionRepository.GetLastVersionAsync();

			Assert.That(versionLast.Version == new TgEfVersionEntity().Version);
			TestContext.WriteLine($"{nameof(versionLast)}.{nameof(versionLast.Version)}: {versionLast.Version}");
			var versions = (await versionRepository.GetListAsync(TgEnumTableTopRecords.All, 0))
				.Items.ToList();
			TestContext.WriteLine($"Found {versions.Count} items");
			await versionRepository.DeleteNewAsync();
			await versionRepository.FillTableVersionsAsync();
		});
	}

	#endregion
}