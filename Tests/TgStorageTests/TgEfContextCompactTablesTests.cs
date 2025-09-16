namespace TgStorageTests;

[TestFixture]
internal sealed class TgEfContextCompactTablesTests : TgStorageTestsBase
{
	#region Methods

	[Test]
	public void Compact_db()
	{
		Assert.DoesNotThrowAsync(StorageManager.ShrinkDbAsync);
	}

	#endregion
}
