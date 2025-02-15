﻿// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
#pragma warning disable NUnit1033

namespace TgStorageTest.Common;

internal abstract class TgDbContextTestsBase
{
	#region Public and private fields, properties, constructor

	protected TgDbContextTestsBase()
	{
		// DI
		var containerBuilder = new ContainerBuilder();
		containerBuilder.RegisterType<TgEfTestContext>().As<ITgEfContext>();
		TgGlobalTools.Container = containerBuilder.Build();
		// Create and update storage
		var task = TgEfUtils.CreateAndUpdateDbAsync();
		task.Wait();
	}

	#endregion

	#region Public and private methods

	//

	#endregion
}