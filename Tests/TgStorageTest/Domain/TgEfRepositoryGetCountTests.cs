// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
#pragma warning disable NUnit1033

namespace TgStorageTest.Domain;

[TestFixture]
internal sealed class TgEfRepositoryGetCountTests : TgDbContextTestsBase
{
	#region Public and private methods

	private void GetCountAsync<TEfEntity>(ITgEfRepository<TEfEntity> repo)
		where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
	{
		Assert.DoesNotThrowAsync(async () =>
		{
			var count = await repo.GetCountAsync();
			TestContext.WriteLine($"Found {count} items.");
		});
	}

	[Test]
	public void TgEf_get_count_apps_async() => GetCountAsync(new TgEfAppRepository());

	[Test]
	public void TgEf_get_count_contacts_async() => GetCountAsync(new TgEfContactRepository());

	[Test]
	public void TgEf_get_count_documents_async() => GetCountAsync(new TgEfDocumentRepository());

	[Test]
	public void TgEf_get_count_filters_async() => GetCountAsync(new TgEfFilterRepository());

	[Test]
	public void TgEf_get_count_messages_async() => GetCountAsync(new TgEfMessageRepository());

	[Test]
	public void TgEf_get_count_proxies_async() => GetCountAsync(new TgEfProxyRepository());

	[Test]
	public void TgEf_get_count_sources_async() => GetCountAsync(new TgEfSourceRepository());

	[Test]
	public void TgEf_get_count_stories_async() => GetCountAsync(new TgEfStoryRepository());

	[Test]
	public void TgEf_get_count_versions_async() => GetCountAsync(new TgEfVersionRepository());

	#endregion
}