//// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
//// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
//#pragma warning disable NUnit1033

//namespace TgStorageTest.Domain;

//[TestFixture]
//internal sealed class TgEfRepositoryCreateNewTests : TgDbContextTestsBase
//{
//	#region Methods

//	[Test]
//    public void Get_table_models()
//    {
//        Assert.DoesNotThrow(() =>
//        {
//            var sqlTables = TgGlobalTools.GetTableModels();
//            foreach (var sqlTable in sqlTables)
//            {
//                TestContext.WriteLine(sqlTable.GetType());
//            }
//        });
//    }

//    [Test]
//    public void Create_new_items_and_delete()
//    {
//        Assert.DoesNotThrowAsync(async () =>
//        {
//            foreach (var sqlType in sqlTypes)
//            {
//                switch (sqlType)
//                {
//                    case var cls when cls == typeof(TgEfAppEntity):
//	                    await CreateNewItemAndDeleteAsync(new TgEfAppRepository());
//						break;
//                    case var cls when cls == typeof(TgEfUserEntity):
//	                    await CreateNewItemAndDeleteAsync(new TgEfUserRepository());
//						break;
//                    case var cls when cls == typeof(TgEfDocumentEntity):
//	                    await CreateNewItemAndDeleteAsync(new TgEfDocumentRepository());
//						break;
//                    case var cls when cls == typeof(TgEfFilterEntity):
//	                    await CreateNewItemAndDeleteAsync(new TgEfFilterRepository());
//						break;
//                    case var cls when cls == typeof(TgEfMessageEntity):
//	                    await CreateNewItemAndDeleteAsync(new TgEfMessageRepository());
//						break;
//                    case var cls when cls == typeof(TgEfProxyEntity):
//	                    await CreateNewItemAndDeleteAsync(new TgEfProxyRepository());
//                        break;
//                    case var cls when cls == typeof(TgEfSourceEntity):
//	                    await CreateNewItemAndDeleteAsync(new TgEfSourceRepository());
//						break;
//                    case var cls when cls == typeof(TgEfStoryEntity):
//	                    await CreateNewItemAndDeleteAsync(new TgEfStoryRepository());
//						break;
//                    case var cls when cls == typeof(TgEfVersionEntity):
//	                    await CreateNewItemAndDeleteAsync(new TgEfVersionRepository());
//						break;
//                }
//                TestContext.WriteLine();
//            }
//        });
//    }

//    private async Task CreateNewItemAndDeleteAsync<TEfEntity>(ITgEfRepository<TEfEntity> repository) where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
//    {
//		TgEfStorageResult<TEfEntity> storageResult = await repository.CreateNewAsync();
//		Assert.That(storageResult.IsExists);
//		TestContext.WriteLine(storageResult.Item.ToDebugString());
//		storageResult = await repository.DeleteAsync(storageResult.Item);
//		Assert.That(!storageResult.IsExists);
//    }

//	[Test]
//    public void Get_new_items_and_delete()
//    {
//        Assert.DoesNotThrowAsync(async () =>
//        {
//            foreach (var sqlType in sqlTypes)
//            {
//				switch (sqlType)
//				{
//					case var cls when cls == typeof(TgEfAppEntity):
//						await GetNewItemsAndDeleteAsync(new TgEfAppRepository());
//						break;
//					case var cls when cls == typeof(TgEfUserEntity):
//						await GetNewItemsAndDeleteAsync(new TgEfUserRepository());
//						break;
//					case var cls when cls == typeof(TgEfDocumentEntity):
//						await GetNewItemsAndDeleteAsync(new TgEfDocumentRepository());
//						break;
//					case var cls when cls == typeof(TgEfFilterEntity):
//						await GetNewItemsAndDeleteAsync(new TgEfFilterRepository());
//						break;
//					case var cls when cls == typeof(TgEfMessageEntity):
//						await GetNewItemsAndDeleteAsync(new TgEfMessageRepository());
//						break;
//					case var cls when cls == typeof(TgEfProxyEntity):
//						await GetNewItemsAndDeleteAsync(new TgEfProxyRepository());
//						break;
//					case var cls when cls == typeof(TgEfSourceEntity):
//						await GetNewItemsAndDeleteAsync(new TgEfSourceRepository());
//						break;
//					case var cls when cls == typeof(TgEfStoryEntity):
//						await GetNewItemsAndDeleteAsync(new TgEfStoryRepository());
//						break;
//					case var cls when cls == typeof(TgEfVersionEntity):
//						await GetNewItemsAndDeleteAsync(new TgEfVersionRepository());
//						break;
//				}
//				TestContext.WriteLine();
//            }
//        });
//    }

//	private static async Task GetNewItemsAndDeleteAsync<TEfEntity>(ITgEfRepository<TEfEntity> repository) where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
//	{
//		TgEfStorageResult<TEfEntity> storageResult;
//		do
//		{
//			storageResult = await repository.GetNewAsync(isReadOnly: false);
//			if (storageResult.IsExists)
//			{
//				TestContext.WriteLine(storageResult.Item.ToDebugString());
//				TgEfStorageResult<TEfEntity> storageResultDelete = await repository.DeleteAsync(storageResult.Item);
//				Assert.That(!storageResultDelete.IsExists);
//			}
//		} while (storageResult.IsExists);
//	}

//	#endregion
//}