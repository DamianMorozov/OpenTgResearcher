#pragma warning disable NUnit1033

namespace TgStorageTests.Domain;

[TestFixture]
internal sealed class TgEfRepositoryGetListWhereTests : TgStorageTestsBase
{
	#region Methods

	private Task GetListWhereAsync<TEfEntity, TDto>(ITgEfRepository<TEfEntity, TDto> repo, TgEnumTableTopRecords count = TgEnumTableTopRecords.Top20)
		where TEfEntity : class, ITgEfEntity, new()
        where TDto : class, ITgDto, new()
    {
		return Assert.DoesNotThrowAsync(async () =>
		{
			TgEfStorageResult<TEfEntity> storageResult = await repo.GetListAsync(count, 0, TgGlobalTools.WhereUidNotEmpty<TEfEntity>());
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
	public Task Get_apps_where_async() => GetListWhereAsync(new TgEfAppRepository());

	[Test]
	public Task Get_contacts_where_async() => GetListWhereAsync(new TgEfUserRepository());

	[Test]
	public Task Get_documents_where_async() => GetListWhereAsync(new TgEfDocumentRepository());

	[Test]
	public Task Get_filters_where_async() => GetListWhereAsync(new TgEfFilterRepository());

	[Test]
	public Task Get_messages_where_async() => GetListWhereAsync(new TgEfMessageRepository());

	[Test]
	public Task Get_messages_relations_where_async() => GetListWhereAsync(new TgEfMessageRelationRepository());

	[Test]
	public Task Get_proxies_where_async() => GetListWhereAsync(new TgEfProxyRepository());

	[Test]
	public Task Get_sources_where_async() => GetListWhereAsync(new TgEfSourceRepository());

	[Test]
	public Task Get_stories_where_async() => GetListWhereAsync(new TgEfStoryRepository());

	[Test]
	public Task Get_versions_where_async() => GetListWhereAsync(new TgEfVersionRepository());

	#endregion
}
