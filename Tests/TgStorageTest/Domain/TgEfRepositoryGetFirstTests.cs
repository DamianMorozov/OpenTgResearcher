// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
#pragma warning disable NUnit1033

namespace TgStorageTest.Domain;

[TestFixture]
internal sealed class TgEfRepositoryGetFirstTests : TgDbContextTestsBase
{
	#region Public and private methods

	private void GetFirst<TEfEntity, TDto>(ITgEfRepository<TEfEntity, TDto> repo) 
		where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
        where TDto : class, ITgDto<TEfEntity, TDto>, new()
    {
		Assert.DoesNotThrowAsync(async () =>
		{
			var item = (await repo.GetListAsync(TgEnumTableTopRecords.Top1, isReadOnly: true, skip: 0)).Items.FirstOrDefault();
			TestContext.WriteLine($"  Found {item?.ToDebugString()}");
		});
	}

	[Test]
	public void TgEf_get_first_app() => GetFirst(new TgEfAppRepository());

	[Test]
	public void TgEf_get_first_contact() => GetFirst(new TgEfUserRepository());

	[Test]
	public void TgEf_get_first_document() => GetFirst(new TgEfDocumentRepository());

	[Test]
	public void TgEf_get_first_filter() => GetFirst(new TgEfFilterRepository());

	[Test]
	public void TgEf_get_first_message() => GetFirst(new TgEfMessageRepository());

	[Test]
	public void TgEf_get_first_proxy() => GetFirst(new TgEfProxyRepository());

	[Test]
	public void TgEf_get_first_source() => GetFirst(new TgEfSourceRepository());

	[Test]
	public void TgEf_get_first_story() => GetFirst(new TgEfStoryRepository());

	[Test]
	public void TgEf_get_first_version() => GetFirst(new TgEfVersionRepository());

	#endregion
}