﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
#pragma warning disable NUnit1033

namespace TgStorageTest.Domain;

[TestFixture]
internal sealed class TgEfRepositoryGetCountTests : TgDbContextTestsBase
{
	#region Public and private methods

	private void GetCountAsync<TEntity>(ITgEfRepository<TEntity> repo) where TEntity : ITgDbFillEntity<TEntity>, new()
	{
		Assert.DoesNotThrowAsync(async () =>
		{
			int count = await repo.GetCountAsync();
			TestContext.WriteLine($"Found {count} items.");
		});
	}

	[Test]
	public void TgEf_get_count_appa_async() => GetCountAsync(new TgEfAppRepository(TgEfUtils.EfContext));

	[Test]
	public void TgEf_get_count_contacts_async() => GetCountAsync(new TgEfContactRepository(TgEfUtils.EfContext));

	[Test]
	public void TgEf_get_count_documents_async() => GetCountAsync(new TgEfDocumentRepository(TgEfUtils.EfContext));

	[Test]
	public void TgEf_get_count_filters_async() => GetCountAsync(new TgEfFilterRepository(TgEfUtils.EfContext));

	[Test]
	public void TgEf_get_count_messages_async() => GetCountAsync(new TgEfMessageRepository(TgEfUtils.EfContext));

	[Test]
	public void TgEf_get_count_proxies_async() => GetCountAsync(new TgEfProxyRepository(TgEfUtils.EfContext));

	[Test]
	public void TgEf_get_count_sources_async() => GetCountAsync(new TgEfSourceRepository(TgEfUtils.EfContext));

	[Test]
	public void TgEf_get_count_stories_async() => GetCountAsync(new TgEfStoryRepository(TgEfUtils.EfContext));

	[Test]
	public void TgEf_get_count_versions_async() => GetCountAsync(new TgEfVersionRepository(TgEfUtils.EfContext));

	#endregion
}