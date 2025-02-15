// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
#pragma warning disable NUnit1033

namespace TgStorageTest.Domain;

[TestFixture]
internal sealed class TgEfSourceRepositorySaveTests : TgDbContextTestsBase
{
	#region Public and private methods

	[Test]
	public void Save_sources_async()
	{
		Assert.DoesNotThrowAsync(async () =>
		{
			TgEfSourceRepository repo = new();
			var storageResult = await repo.GetFirstAsync();
			if (storageResult.IsExists)
			{
				var itemFind = await repo.GetItemAsync(storageResult.Item);
				Assert.That(itemFind, Is.Not.Null);
				TestContext.WriteLine(itemFind.ToDebugString());
				// Save
				storageResult = await repo.SaveAsync(itemFind);
				Assert.That(storageResult.IsExists);
			}
		});
	}

	#endregion
}