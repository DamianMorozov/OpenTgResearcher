#pragma warning disable NUnit1033

namespace TgStorageTests.Utils;

[TestFixture]
internal sealed class TgStorageTestsUtils : TgStorageTestsBase
{
	#region Methods

	[Test]
	public void Check_version_last_async()
	{
		Assert.DoesNotThrowAsync(async () =>
		{
			TgEfVersionRepository versionRepository = new();
			await versionRepository.FillTableVersionsAsync();

			var versionLast = await versionRepository.GetLastVersionAsync();

			Assert.That(versionLast.Version == versionRepository.LastVersion);
			TestContext.WriteLine($"  {nameof(versionLast)}.{nameof(versionLast.Version)}: {versionLast.Version}");
			var versions = (await versionRepository.GetListAsync(TgEnumTableTopRecords.All, 0))
				.Items.ToList();
			TestContext.WriteLine($"  Found {versions.Count()} items");
		});
	}

	[Test]
	public void Check_version_last_with_add_default_async()
	{
		Assert.DoesNotThrowAsync(async () =>
		{
			TgEfVersionRepository versionRepository = new();
			await versionRepository.FillTableVersionsAsync();

			var versionDto = new TgEfVersionDto();
			await versionRepository.SaveAsync(versionDto);
			var versionLast = await versionRepository.GetLastVersionAsync();

			Assert.That(versionLast.Version == versionRepository.LastVersion);
			TestContext.WriteLine($"  {nameof(versionLast)}.{nameof(versionLast.Version)}: {versionLast.Version}");
			var versions = (await versionRepository.GetListAsync(TgEnumTableTopRecords.All, 0))
				.Items.ToList();
			TestContext.WriteLine($"  Found {versions.Count} items");
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
			var versionDto = new TgEfVersionDto();
			await versionRepository.SaveAsync(versionDto);
			var versionLast = await versionRepository.GetLastVersionAsync();

			Assert.That(versionLast.Version == new TgEfVersionEntity().Version);
			TestContext.WriteLine($"  {nameof(versionLast)}.{nameof(versionLast.Version)}: {versionLast.Version}");
			var versions = (await versionRepository.GetListAsync(TgEnumTableTopRecords.All, 0))
				.Items.ToList();
			TestContext.WriteLine($"  Found {versions.Count} items");
			await versionRepository.DeleteNewAsync();
			await versionRepository.FillTableVersionsAsync();
		});
	}

	#endregion
}
