#pragma warning disable NUnit1033

namespace TgStorageTests.Domain;

[TestFixture]
internal sealed class TgEfRepositoryGetCountWhereTests : TgStorageTestsBase
{
	#region Methods

	private Task GetCountWhereAsync<TEfEntity, TDto>(ITgEfRepository<TEfEntity, TDto> repo)
		where TEfEntity : class, ITgEfEntity, new()
        where TDto : class, ITgDto, new()
    {
		return Assert.DoesNotThrowAsync(async () =>
		{
			var count = await repo.GetCountAsync(TgGlobalTools.WhereUidNotEmpty<TEfEntity>());
			TestContext.WriteLine($"  Found {count} items.");
		});
	}

	[Test]
	public Task TgEf_get_count_apps_async() => GetCountWhereAsync(new TgEfAppRepository());

	[Test]
	public Task TgEf_get_count_contacts_async() => GetCountWhereAsync(new TgEfUserRepository());

	[Test]
	public Task TgEf_get_count_documents_async() => GetCountWhereAsync(new TgEfDocumentRepository());

	[Test]
	public Task TgEf_get_count_filters_async() => GetCountWhereAsync(new TgEfFilterRepository());

	[Test]
	public Task TgEf_get_count_messages_async() => GetCountWhereAsync(new TgEfMessageRepository());

	[Test]
	public Task TgEf_get_count_messages_relations_async() => GetCountWhereAsync(new TgEfMessageRelationRepository());

	[Test]
	public Task TgEf_get_count_proxies_async() => GetCountWhereAsync(new TgEfProxyRepository());

	[Test]
	public Task TgEf_get_count_sources_async() => GetCountWhereAsync(new TgEfSourceRepository());

	[Test]
	public Task TgEf_get_count_stories_async() => GetCountWhereAsync(new TgEfStoryRepository());

	[Test]
	public Task TgEf_get_count_versions_async() => GetCountWhereAsync(new TgEfVersionRepository());

	#endregion
}
