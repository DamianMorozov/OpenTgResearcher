#pragma warning disable NUnit1033

namespace TgStorageTests.Domain;

[TestFixture]
internal sealed class TgEfRepositoryGetCountTests : TgStorageTestsBase
{
	#region Methods

	private Task GetCountAsync<TEfEntity, TDto>(ITgEfRepository<TEfEntity, TDto> repo)
		where TEfEntity : class, ITgEfEntity, new()
        where TDto : class, ITgDto, new()
    {
		return Assert.DoesNotThrowAsync(async () =>
		{
			var count = await repo.GetCountAsync();
			TestContext.WriteLine($"  Found {count} items.");
		});
	}

	[Test]
	public Task TgEf_get_count_apps_async() => GetCountAsync(new TgEfAppRepository());

	[Test]
	public Task TgEf_get_count_contacts_async() => GetCountAsync(new TgEfUserRepository());

	[Test]
	public Task TgEf_get_count_documents_async() => GetCountAsync(new TgEfDocumentRepository());

	[Test]
	public Task TgEf_get_count_filters_async() => GetCountAsync(new TgEfFilterRepository());

	[Test]
	public Task TgEf_get_count_messages_async() => GetCountAsync(new TgEfMessageRepository());

	[Test]
	public Task TgEf_get_count_messages_relations_async() => GetCountAsync(new TgEfMessageRelationRepository());

	[Test]
	public Task TgEf_get_count_proxies_async() => GetCountAsync(new TgEfProxyRepository());

	[Test]
	public Task TgEf_get_count_sources_async() => GetCountAsync(new TgEfSourceRepository());

	[Test]
	public Task TgEf_get_count_stories_async() => GetCountAsync(new TgEfStoryRepository());

	[Test]
	public Task TgEf_get_count_versions_async() => GetCountAsync(new TgEfVersionRepository());

	#endregion
}
