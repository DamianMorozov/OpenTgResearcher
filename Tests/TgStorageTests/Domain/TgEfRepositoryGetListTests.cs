#pragma warning disable NUnit1033

namespace TgStorageTests.Domain;

[TestFixture]
internal sealed class TgEfRepositoryGetListTests : TgStorageTestsBase
{
	#region Methods

	private Task GetListAsync<TEfEntity, TDto>(ITgEfRepository<TEfEntity, TDto> repo, TgEnumTableTopRecords count = TgEnumTableTopRecords.Top20)
		where TEfEntity : class, ITgEfEntity, new()
        where TDto : class, ITgDto, new()
    {
		return Assert.DoesNotThrowAsync(async () =>
		{
			TgEfStorageResult<TEfEntity> storageResult = await repo.GetListAsync(count, 0);
			TestContext.WriteLine($"  Found {storageResult.Items.Count()} items.");
			foreach (var item in storageResult.Items)
			{
				var itemFind = await repo.GetItemAsync(item);
				Assert.That(itemFind, Is.Not.Null);
                TestContext.WriteLine(itemFind.ToDebugString());
			}
		});
	}

	[Test]
	public Task Get_apps_async() => GetListAsync(new TgEfAppRepository());

	[Test]
	public Task Get_contacts_async() => GetListAsync(new TgEfUserRepository());

	[Test]
	public Task Get_documents_async() => GetListAsync(new TgEfDocumentRepository());

	[Test]
	public Task Get_filters_async() => GetListAsync(new TgEfFilterRepository());

	[Test]
	public Task Get_messages_async() => GetListAsync(new TgEfMessageRepository());

	[Test]
	public Task Get_messages_relations_async() => GetListAsync(new TgEfMessageRelationRepository());

	[Test]
	public Task Get_proxies_async() => GetListAsync(new TgEfProxyRepository());

	[Test]
	public Task Get_sources_async() => GetListAsync(new TgEfSourceRepository());

	[Test]
	public Task Get_stories_async() => GetListAsync(new TgEfStoryRepository());

	[Test]
	public Task Get_versions_async() => GetListAsync(new TgEfVersionRepository());

	#endregion
}
