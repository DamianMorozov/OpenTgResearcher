#pragma warning disable NUnit1033

namespace TgStorageTests.Domain;

[TestFixture]
internal sealed class TgEfRepositoryGetFirstTests : TgStorageTestsBase
{
	#region Methods

	private Task GetFirst<TEfEntity, TDto>(ITgEfRepository<TEfEntity, TDto> repo)
		where TEfEntity : class, ITgEfEntity, new()
        where TDto : class, ITgDto, new()
    {
		return Assert.DoesNotThrowAsync(async () =>
		{
			var item = (await repo.GetListAsync(TgEnumTableTopRecords.Top1, isReadOnly: true, skip: 0)).Items.FirstOrDefault();
			TestContext.WriteLine($"  Found {item?.ToDebugString()}");
		});
	}

	[Test]
	public Task TgEf_get_first_app() => GetFirst(new TgEfAppRepository());

	[Test]
	public Task TgEf_get_first_contact() => GetFirst(new TgEfUserRepository());

	[Test]
	public Task TgEf_get_first_document() => GetFirst(new TgEfDocumentRepository());

	[Test]
	public Task TgEf_get_first_filter() => GetFirst(new TgEfFilterRepository());

	[Test]
	public Task TgEf_get_first_message() => GetFirst(new TgEfMessageRepository());

	[Test]
	public Task TgEf_get_first_message_relation() => GetFirst(new TgEfMessageRelationRepository());

	[Test]
	public Task TgEf_get_first_proxy() => GetFirst(new TgEfProxyRepository());

	[Test]
	public Task TgEf_get_first_source() => GetFirst(new TgEfSourceRepository());

	[Test]
	public Task TgEf_get_first_story() => GetFirst(new TgEfStoryRepository());

	[Test]
	public Task TgEf_get_first_version() => GetFirst(new TgEfVersionRepository());

	#endregion
}
