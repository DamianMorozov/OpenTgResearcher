// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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