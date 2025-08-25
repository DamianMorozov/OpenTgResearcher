//// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
//// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

//namespace TgStorageTest;

//[TestFixture]
//internal sealed class TgEfContextAlterTablesTests : TgDbContextTestsBase
//{
//	#region Methods

//	[Test]
//	public void Alter_apps_no_case_uid()
//	{
//		Assert.DoesNotThrow(() =>
//		{
//			TgEfContext efContext = CreateEfContext();
//			TgEfStorageResult<TgEfAppEntity> storageResultGet = new TgEfAppRepository(efContext).GetList(TgEnumTableTopRecords.All, );
//			TgEfStorageResult<TgEfAppEntity> storageResult = efContext.AlterTableNoCaseUid<TgEfAppEntity>();
//			Assert.That(storageResultGet.IsExists ? storageResult.State == TgEnumEntityState.IsExecuted : storageResult.State == TgEnumEntityState.NotExecuted);
//		});
//	}

//	[Test]
//	public void Alter_documents_no_case_uid()
//	{
//		Assert.DoesNotThrow(() =>
//		{
//			TgEfContext efContext = CreateEfContext();
//			TgEfStorageResult<TgEfDocumentEntity> storageResultGet = new TgEfDocumentRepository(efContext).GetList(TgEnumTableTopRecords.All, );
//			TgEfStorageResult<TgEfDocumentEntity> storageResult = efContext.AlterTableNoCaseUid<TgEfDocumentEntity>();
//			Assert.That(storageResultGet.IsExists ? storageResult.State == TgEnumEntityState.IsExecuted : storageResult.State == TgEnumEntityState.NotExecuted);
//		});
//	}

//	[Test]
//	public void Alter_filters_no_case_uid()
//	{
//		Assert.DoesNotThrow(() =>
//		{
//			TgEfContext efContext = CreateEfContext();
//			TgEfStorageResult<TgEfFilterEntity> storageResultGet = new TgEfFilterRepository(efContext).GetList(TgEnumTableTopRecords.All, );
//			TgEfStorageResult<TgEfFilterEntity> storageResult = efContext.AlterTableNoCaseUid<TgEfFilterEntity>();
//			Assert.That(storageResultGet.IsExists ? storageResult.State == TgEnumEntityState.IsExecuted : storageResult.State == TgEnumEntityState.NotExecuted);
//		});
//	}

//	[Test]
//	public void Alter_messages_no_case_uid()
//	{
//		Assert.DoesNotThrow(() =>
//		{
//			TgEfContext efContext = CreateEfContext();
//			TgEfStorageResult<TgEfMessageEntity> storageResultGet = new TgEfMessageRepository(efContext).GetList(TgEnumTableTopRecords.All, );
//			TgEfStorageResult<TgEfMessageEntity> storageResult = efContext.AlterTableNoCaseUid<TgEfMessageEntity>();
//			Assert.That(storageResultGet.IsExists ? storageResult.State == TgEnumEntityState.IsExecuted : storageResult.State == TgEnumEntityState.NotExecuted);
//		});
//	}

//	[Test]
//	public void Alter_proxies_no_case_uid()
//	{
//		Assert.DoesNotThrow(() =>
//		{
//			TgEfContext efContext = CreateEfContext();
//			TgEfStorageResult<TgEfProxyEntity> storageResultGet = new TgEfProxyRepository(efContext).GetList(TgEnumTableTopRecords.All, );
//			TgEfStorageResult<TgEfProxyEntity> storageResult = efContext.AlterTableNoCaseUid<TgEfProxyEntity>();
//			Assert.That(storageResultGet.IsExists ? storageResult.State == TgEnumEntityState.IsExecuted : storageResult.State == TgEnumEntityState.NotExecuted);
//		});
//	}

//	[Test]
//	public void Alter_sources_no_case_uid()
//	{
//		Assert.DoesNotThrow(() =>
//		{
//			TgEfContext efContext = CreateEfContext();
//			TgEfStorageResult<TgEfSourceEntity> storageResultGet = new TgEfSourceRepository(efContext).GetList(TgEnumTableTopRecords.All, );
//			TgEfStorageResult<TgEfSourceEntity> storageResult = efContext.AlterTableNoCaseUid<TgEfSourceEntity>();
//			Assert.That(storageResultGet.IsExists ? storageResult.State == TgEnumEntityState.IsExecuted : storageResult.State == TgEnumEntityState.NotExecuted);
//		});
//	}

//	[Test]
//	public void Alter_versions_no_case_uid()
//	{
//		Assert.DoesNotThrow(() =>
//		{
//			TgEfContext efContext = CreateEfContext();
//			TgEfStorageResult<TgEfVersionEntity> storageResultGet = new TgEfVersionRepository(efContext).GetList(TgEnumTableTopRecords.All, );
//			TgEfStorageResult<TgEfVersionEntity> storageResult = efContext.AlterTableNoCaseUid<TgEfVersionEntity>();
//			Assert.That(storageResultGet.IsExists ? storageResult.State == TgEnumEntityState.IsExecuted : storageResult.State == TgEnumEntityState.NotExecuted);
//		});
//	}

//	#endregion
//}