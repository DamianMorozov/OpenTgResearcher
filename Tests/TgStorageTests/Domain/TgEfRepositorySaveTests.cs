#pragma warning disable NUnit1033

namespace TgStorageTests.Domain;

[TestFixture]
internal sealed class TgEfRepositorySaveTests : TgStorageTestsBase
{
	#region Methods

	private Task SaveItemAsync<TEfEntity, TDto>(ITgEfRepository<TEfEntity, TDto> repo)
		where TEfEntity : class, ITgEfEntity, new()
        where TDto : class, ITgDto, new()
    {
		return Assert.DoesNotThrowAsync(async () =>
		{
			var dtos = await repo.GetListDtosAsync(take: 1, skip: 0);
			if (dtos.Count == 1)
			{
				var dto = dtos.FirstOrDefault();
				Assert.That(dto, Is.Not.Null);
				TestContext.WriteLine(dto.ToDebugString());
            }
		});
	}

	[Test]
	public Task Save_apps_async() => SaveItemAsync(new TgEfAppRepository());

	[Test]
	public Task Save_contacts_async() => SaveItemAsync(new TgEfUserRepository());

	[Test]
	public Task Save_documents_async() => SaveItemAsync(new TgEfDocumentRepository());

	[Test]
	public Task Save_filters_async() => SaveItemAsync(new TgEfFilterRepository());

	[Test]
	public Task Save_messages_async() => SaveItemAsync(new TgEfMessageRepository());

	[Test]
	public Task Save_messages_relations_async() => SaveItemAsync(new TgEfMessageRelationRepository());

	[Test]
	public Task Save_proxies_async() => SaveItemAsync(new TgEfProxyRepository());

	[Test]
	public Task Save_sources_async() => SaveItemAsync(new TgEfSourceRepository());

	[Test]
	public Task Save_stories_async() => SaveItemAsync(new TgEfStoryRepository());

	[Test]
	public Task Save_versions_async() => SaveItemAsync(new TgEfVersionRepository());

	#endregion
}
