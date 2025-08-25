// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
#pragma warning disable NUnit1033

namespace TgStorageTest.Domain;

[TestFixture]
internal sealed class TgEfSourceRepositorySaveTests : TgDbContextTestsBase
{
	#region Methods

	[Test]
	public void Save_sources_async()
	{
		Assert.DoesNotThrowAsync(async () =>
		{
			TgEfSourceRepository repo = new();
			var storageResult = (await repo.GetListAsync(TgEnumTableTopRecords.Top1, isReadOnly: false, skip: 0));
			if (storageResult.IsExists)
			{
				var itemFind = storageResult.Items.FirstOrDefault();
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