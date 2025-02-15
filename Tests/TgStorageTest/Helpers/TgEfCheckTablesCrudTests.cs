//// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
//// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

//namespace TgStorageTest.Helpers;

//[TestFixture]
//internal class TgEfCheckTablesCrudTests : TgDbContextTestsBase
//{
//	#region Public and private methods

//	[Test]
//	public void Check_table_apps_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgEfUtils.CheckTableProxiesCrudAsync(efContext));
//			Assert.That(await TgEfUtils.CheckTableAppsCrudAsync(efContext));
//		});
//	}

//    [Test]
//	public void Check_table_contacts_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgEfUtils.CheckTableContactsCrudAsync(efContext));
//		});
//	}

//    [Test]
//	public void Check_table_documents_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgEfUtils.CheckTableDocumentsCrudAsync(efContext));
//		});
//	}

//	[Test]
//	public void Check_table_filters_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgEfUtils.CheckTableFiltersCrudAsync(efContext));
//		});
//	}

//	[Test]
//	public void Check_table_messages_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgEfUtils.CheckTableMessagesCrudAsync(efContext));
//		});
//	}

//	[Test]
//	public void Check_table_proxies_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgEfUtils.CheckTableProxiesCrudAsync(efContext));
//		});
//	}

//	[Test]
//	public void Check_table_sources_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgEfUtils.CheckTableSourcesCrudAsync(efContext));
//		});
//	}

//	[Test]
//	public void Check_table_stories_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgEfUtils.CheckTableStoriesCrudAsync(efContext));
//		});
//	}

//	[Test]
//	public void Check_table_versions_crud_async()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			using var efContext = CreateEfTestContext();
//			Assert.That(await TgEfUtils.CheckTableVersionsCrudAsync(efContext));
//		});
//	}

//	#endregion
//}