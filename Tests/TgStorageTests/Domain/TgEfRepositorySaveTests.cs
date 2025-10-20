#pragma warning disable NUnit1033

namespace TgStorageTests.Domain;

[TestFixture]
internal sealed class TgEfRepositorySaveTests : TgStorageTestsBase
{
	#region Methods

	private void SaveItemAsync<TEfEntity, TDto>(ITgEfRepository<TEfEntity, TDto> repo) 
		where TEfEntity : class, ITgEfEntity, new()
        where TDto : class, ITgDto, new()
    {
		Assert.DoesNotThrowAsync(async () =>
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
	public void Save_apps_async() => SaveItemAsync(new TgEfAppRepository());

	[Test]
	public void Save_contacts_async() => SaveItemAsync(new TgEfUserRepository());

	[Test]
	public void Save_documents_async() => SaveItemAsync(new TgEfDocumentRepository());

	[Test]
	public void Save_filters_async() => SaveItemAsync(new TgEfFilterRepository());

	[Test]
	public void Save_messages_async() => SaveItemAsync(new TgEfMessageRepository());

	[Test]
	public void Save_messages_relations_async() => SaveItemAsync(new TgEfMessageRelationRepository());

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
