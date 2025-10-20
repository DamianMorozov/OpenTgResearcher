#pragma warning disable NUnit1033

namespace TgStorageTests.Domain;

[TestFixture]
internal sealed class TgEfSourceRepositorySaveTests : TgStorageTestsBase
{
	#region Methods

	[Test]
	public void Save_sources_async()
	{
		Assert.DoesNotThrowAsync(async () =>
		{
			TgEfSourceRepository repo = new();
			var dtos = (await repo.GetListDtosAsync(take: 1, skip: 0));
			if (dtos.Count == 1)
			{
				var dto = dtos.FirstOrDefault();
				Assert.That(dto, Is.Not.Null);
				TestContext.WriteLine(dto.ToDebugString());
			}
		});
	}

	#endregion
}
