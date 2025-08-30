//// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
//// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

//namespace TgStorageTests.Helpers;

//[TestFixture]
//internal class TgEfCheckTablesCrudTests : TgDbContextTestsBase
//{
//	#region Methods

//	[Test]
//	public void Check_table_apps_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgGlobalTools.CheckTableProxiesCrudAsync(efContext));
//			Assert.That(await TgGlobalTools.CheckTableAppsCrudAsync(efContext));
//		});
//	}

//    [Test]
//	public void Check_table_contacts_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgGlobalTools.CheckTableContactsCrudAsync(efContext));
//		});
//	}

//    [Test]
//	public void Check_table_documents_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgGlobalTools.CheckTableDocumentsCrudAsync(efContext));
//		});
//	}

//	[Test]
//	public void Check_table_filters_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgGlobalTools.CheckTableFiltersCrudAsync(efContext));
//		});
//	}

//	[Test]
//	public void Check_table_messages_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgGlobalTools.CheckTableMessagesCrudAsync(efContext));
//		});
//	}

//	[Test]
//	public void Check_table_proxies_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgGlobalTools.CheckTableProxiesCrudAsync(efContext));
//		});
//	}

//	[Test]
//	public void Check_table_sources_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgGlobalTools.CheckTableSourcesCrudAsync(efContext));
//		});
//	}

//	[Test]
//	public void Check_table_stories_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgGlobalTools.CheckTableStoriesCrudAsync(efContext));
//		});
//	}

//	[Test]
//	public void Check_table_versions_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgGlobalTools.CheckTableVersionsCrudAsync(efContext));
//		});
//	}

//	#endregion
//}