namespace TgStorageTests;

[TestFixture]
internal sealed class TgEfContextCompactTablesTests : TgStorageTestsBase
{
	#region Methods

	[Test]
	public Task Compact_db()
	{
		return Assert.DoesNotThrowAsync(StorageManager.ShrinkDbAsync);
	}

	#endregion
}
