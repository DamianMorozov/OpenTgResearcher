//// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
//// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

//namespace TgStorageTest;

//[TestFixture]
//internal sealed class TgEfContextUpdateTablesTests : TgDbContextTestsBase
//{
//	#region Methods

//	[Test]
//	public void Upper_uid_at_apps()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			TgEfStorageResult<TgEfAppEntity> storageResult = await new TgEfAppRepository(TgGlobalTools.EfContext).GetFirstAsync();
//			if (storageResult.IsExists)
//			{
//				storageResult = await CreateEfContext().UpdateTableUidUpperCaseAsync<TgEfAppEntity>(storageResult.Item.Uid);
//				Assert.That(storageResult.State == TgEnumEntityState.IsExecuted);
//			}
//		});
//	}

//	[Test]
//	public void Upper_uid_at_documents()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			TgEfStorageResult<TgEfDocumentEntity> storageResult = await new TgEfDocumentRepository(TgGlobalTools.EfContext).GetFirstAsync();
//			if (storageResult.IsExists)
//			{
//				storageResult = await CreateEfContext().UpdateTableUidUpperCaseAsync<TgEfDocumentEntity>(storageResult.Item.Uid);
//				Assert.That(storageResult.State == TgEnumEntityState.IsExecuted);
//			}
//		});
//	}

//	[Test]
//	public void Upper_uid_at_filters()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			TgEfStorageResult<TgEfFilterEntity> storageResult = await new TgEfFilterRepository(TgGlobalTools.EfContext).GetFirstAsync();
//			if (storageResult.IsExists)
//			{
//				storageResult = await CreateEfContext().UpdateTableUidUpperCaseAsync<TgEfFilterEntity>(storageResult.Item.Uid);
//				Assert.That(storageResult.State == TgEnumEntityState.IsExecuted);
//			}
//		});
//	}

//	[Test]
//	public void Upper_uid_at_messages()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			TgEfStorageResult<TgEfMessageEntity> storageResult = await new TgEfMessageRepository(TgGlobalTools.EfContext).GetFirstAsync();
//			if (storageResult.IsExists)
//			{
//				storageResult = await CreateEfContext().UpdateTableUidUpperCaseAsync<TgEfMessageEntity>(storageResult.Item.Uid);
//				Assert.That(storageResult.State == TgEnumEntityState.IsExecuted);
//			}
//		});
//	}

//	[Test]
//	public void Upper_uid_at_proxies()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			TgEfStorageResult<TgEfProxyEntity> storageResult = await new TgEfProxyRepository(TgGlobalTools.EfContext).GetFirstAsync();
//			if (storageResult.IsExists)
//			{
//				storageResult = await CreateEfContext().UpdateTableUidUpperCaseAsync<TgEfProxyEntity>(storageResult.Item.Uid);
//				Assert.That(storageResult.State == TgEnumEntityState.IsExecuted);
//			}
//		});
//	}

//	[Test]
//	public void Upper_uid_at_sources()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			TgEfStorageResult<TgEfSourceEntity> storageResult = await new TgEfSourceRepository(TgGlobalTools.EfContext).GetFirstAsync();
//			if (storageResult.IsExists)
//			{
//				storageResult = await CreateEfContext().UpdateTableUidUpperCaseAsync<TgEfSourceEntity>(storageResult.Item.Uid);
//				Assert.That(storageResult.State == TgEnumEntityState.IsExecuted);
//			}
//		});
//	}

//	[Test]
//	public void Upper_uid_at_versions()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			TgEfStorageResult<TgEfVersionEntity> storageResult = await new TgEfVersionRepository(TgGlobalTools.EfContext).GetFirstAsync();
//			if (storageResult.IsExists)
//			{
//				storageResult = await CreateEfContext().UpdateTableUidUpperCaseAsync<TgEfVersionEntity>(storageResult.Item.Uid);
//				Assert.That(storageResult.State == TgEnumEntityState.IsExecuted);
//			}
//		});
//	}

//	#endregion
//}