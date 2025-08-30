//// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
//// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

//namespace TgStorageTests.Helpers;

//[TestFixture]
//internal class TgXpoCheckTablesTests : TgDbContextTestsBase
//{
//	#region Methods

//	[Test]
//	public void Check_table_apps()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			Assert.That(await XpoProdContext.CheckTableProxiesAsync());
//			Assert.That(await XpoProdContext.CheckTableAppsAsync());
//		});
//	}

//    [Test]
//	public void Check_table_documents()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			Assert.That(await XpoProdContext.CheckTableDocumentsAsync());
//		});
//	}

//	[Test]
//	public void Check_table_filters()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			Assert.That(await XpoProdContext.CheckTableFiltersAsync());
//		});
//	}

//	[Test]
//	public void Check_table_messages()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			Assert.That(await XpoProdContext.CheckTableMessagesAsync());
//		});
//	}

//	[Test]
//	public void Check_table_proxies()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			Assert.That(await XpoProdContext.CheckTableProxiesAsync());
//		});
//	}

//	[Test]
//	public void Check_table_sources()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			Assert.That(await XpoProdContext.CheckTableSourcesAsync());
//		});
//	}

//	[Test]
//	public void Check_table_versions()
//	{
//		Assert.DoesNotThrowAsync(async () =>
//		{
//			Assert.That(await XpoProdContext.CheckTableVersionsAsync());
//		});
//	}

//	#endregion
//}