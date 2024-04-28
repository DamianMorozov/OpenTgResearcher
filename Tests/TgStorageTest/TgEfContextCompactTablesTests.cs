﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorageTest;

[TestFixture]
internal class TgEfContextCompactTablesTests : TgDbContextTestsBase
{
	#region Public and private methods

	[Test]
	public void Compact_db()
	{
		Assert.DoesNotThrowAsync(async () =>
		{
			await EfProdContext.CompactDbAsync();
		});
	}

	#endregion
}