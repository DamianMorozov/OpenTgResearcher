#pragma warning disable NUnit1033

namespace TgStorageTests.Domain;

[TestFixture]
internal sealed class TgEfRepositoryGetListWhereTests : TgStorageTestsBase
{
	#region Methods

	private void GetListWhereAsync<TEfEntity, TDto>(ITgEfRepository<TEfEntity, TDto> repo, TgEnumTableTopRecords count = TgEnumTableTopRecords.Top20) 
		where TEfEntity : class, ITgEfEntity, new()
        where TDto : class, ITgDto, new()
    {
		Assert.DoesNotThrowAsync(async () =>
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
	public void Get_apps_where_async() => GetListWhereAsync(new TgEfAppRepository());

	[Test]
	public void Get_contacts_where_async() => GetListWhereAsync(new TgEfUserRepository());

	[Test]
	public void Get_documents_where_async() => GetListWhereAsync(new TgEfDocumentRepository());

	[Test]
	public void Get_filters_where_async() => GetListWhereAsync(new TgEfFilterRepository());

	[Test]
	public void Get_messages_where_async() => GetListWhereAsync(new TgEfMessageRepository());

	[Test]
	public void Get_messages_relations_where_async() => GetListWhereAsync(new TgEfMessageRelationRepository());

	[Test]
	public void Get_proxies_where_async() => GetListWhereAsync(new TgEfProxyRepository());

	[Test]
	public void Get_sources_where_async() => GetListWhereAsync(new TgEfSourceRepository());

	[Test]
	public void Get_stories_where_async() => GetListWhereAsync(new TgEfStoryRepository());

	[Test]
	public void Get_versions_where_async() => GetListWhereAsync(new TgEfVersionRepository());

	#endregion
}
