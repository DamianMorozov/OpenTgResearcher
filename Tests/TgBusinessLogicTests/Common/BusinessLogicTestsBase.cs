// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
#pragma warning disable NUnit1033

namespace TgBusinessLogicTests.Common;

internal abstract class BusinessLogicTestsBase : TgStorageTestsBase
{
    #region Fields, properties, constructor

    protected ITgBusinessLogicManager BusinessLogicManager { get; set; }

    protected BusinessLogicTestsBase(Action<ContainerBuilder>? registerTypes = null) : base(cb => RegisterBusinessLogicTypes(cb, registerTypes), isRegisterEfContext: false)
    {
        BusinessLogicManager = Scope.Resolve<ITgBusinessLogicManager>();
    }

    #endregion

    #region Methods

    private static void RegisterBusinessLogicTypes(ContainerBuilder cb, Action<ContainerBuilder>? registerTypes)
    {
        registerTypes?.Invoke(cb);
        // Add registering business logic types here
        cb.RegisterType<TgEfTestContext>().As<ITgEfContext>().InstancePerLifetimeScope();
    }

    #endregion
}
