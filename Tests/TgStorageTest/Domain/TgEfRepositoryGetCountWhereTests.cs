// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
#pragma warning disable NUnit1033

namespace TgStorageTest.Domain;

[TestFixture]
internal sealed class TgEfRepositoryGetCountWhereTests : TgDbContextTestsBase
{
	#region Public and private methods

	private void GetCountWhereAsync<TEfEntity, TDto>(ITgEfRepository<TEfEntity, TDto> repo) 
		where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
        where TDto : class, ITgDto<TEfEntity, TDto>, new()
    {
		Assert.DoesNotThrowAsync(async () =>
		{
			var count = await repo.GetCountAsync(TgGlobalTools.WhereUidNotEmpty<TEfEntity>());
			TestContext.WriteLine($"  Found {count} items.");
		});
	}

	[Test]
	public void TgEf_get_count_apps_async() => GetCountWhereAsync(new TgEfAppRepository());

	[Test]
	public void TgEf_get_count_contacts_async() => GetCountWhereAsync(new TgEfContactRepository());

	[Test]
	public void TgEf_get_count_documents_async() => GetCountWhereAsync(new TgEfDocumentRepository());

	[Test]
	public void TgEf_get_count_filters_async() => GetCountWhereAsync(new TgEfFilterRepository());

	[Test]
	public void TgEf_get_count_messages_async() => GetCountWhereAsync(new TgEfMessageRepository());

	[Test]
	public void TgEf_get_count_proxies_async() => GetCountWhereAsync(new TgEfProxyRepository());

	[Test]
	public void TgEf_get_count_sources_async() => GetCountWhereAsync(new TgEfSourceRepository());

	[Test]
	public void TgEf_get_count_stories_async() => GetCountWhereAsync(new TgEfStoryRepository());

	[Test]
	public void TgEf_get_count_versions_async() => GetCountWhereAsync(new TgEfVersionRepository());

	#endregion
}