// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
#pragma warning disable NUnit1033

namespace TgStorageTest.Domain;

[TestFixture]
internal sealed class TgEfRepositorySaveTests : TgDbContextTestsBase
{
	#region Public and private methods

	private void SaveItemAsync<TEntity>(ITgEfRepository<TEntity> repo) 
		where TEntity : ITgDbFillEntity<TEntity>, new()
	{
		Assert.DoesNotThrowAsync(async () =>
		{
			TgEfStorageResult<TEntity> storageResult = await repo.GetFirstAsync();
			if (storageResult.IsExists)
			{
				var itemFind = await repo.GetItemAsync(storageResult.Item, isReadOnly: false);
				Assert.That(itemFind, Is.Not.Null);
				TestContext.WriteLine(itemFind.ToDebugString());
				// Save
				storageResult = await repo.SaveAsync(itemFind);
				Assert.That(storageResult.IsExists);
			}
		});
	}

	[Test]
	public void Save_apps_async() => SaveItemAsync(new TgEfAppRepository());

	[Test]
	public void Save_contacts_async() => SaveItemAsync(new TgEfContactRepository());

	[Test]
	public void Save_documents_async() => SaveItemAsync(new TgEfDocumentRepository());

	[Test]
	public void Save_filters_async() => SaveItemAsync(new TgEfFilterRepository());

	[Test]
	public void Save_messages_async() => SaveItemAsync(new TgEfMessageRepository());

	[Test]
	public void Save_proxies_async() => SaveItemAsync(new TgEfProxyRepository());

	[Test]
	public void Save_sources_async() => SaveItemAsync(new TgEfSourceRepository());

	[Test]
	public void Save_stories_async() => SaveItemAsync(new TgEfStoryRepository());

	[Test]
	public void Save_versions_async() => SaveItemAsync(new TgEfVersionRepository());

	#endregion
}