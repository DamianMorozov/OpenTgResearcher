// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
#pragma warning disable NUnit1033

namespace TgStorageTests.Domain;

[TestFixture]
internal sealed class TgEfRepositoryGetListTests : TgStorageTestsBase
{
	#region Methods

	private void GetListAsync<TEfEntity, TDto>(ITgEfRepository<TEfEntity, TDto> repo, TgEnumTableTopRecords count = TgEnumTableTopRecords.Top20) 
		where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
        where TDto : class, ITgDto<TEfEntity, TDto>, new()
    {
		Assert.DoesNotThrowAsync(async () =>
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
	public void Get_apps_async() => GetListAsync(new TgEfAppRepository());

	[Test]
	public void Get_contacts_async() => GetListAsync(new TgEfUserRepository());

	[Test]
	public void Get_documents_async() => GetListAsync(new TgEfDocumentRepository());

	[Test]
	public void Get_filters_async() => GetListAsync(new TgEfFilterRepository());

	[Test]
	public void Get_messages_async() => GetListAsync(new TgEfMessageRepository());

	[Test]
	public void Get_messages_relations_async() => GetListAsync(new TgEfMessageRelationRepository());

	[Test]
	public void Get_proxies_async() => GetListAsync(new TgEfProxyRepository());

	[Test]
	public void Get_sources_async() => GetListAsync(new TgEfSourceRepository());

	[Test]
	public void Get_stories_async() => GetListAsync(new TgEfStoryRepository());

	[Test]
	public void Get_versions_async() => GetListAsync(new TgEfVersionRepository());

	#endregion
}