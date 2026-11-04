#pragma warning disable NUnit1033

namespace TgBusinessLogicTests.Common;

internal abstract class BusinessLogicTestsBase : TgStorageTestsBase
{
    #region Fields, properties, constructor

    protected ITgBusinessLogicManager BusinessLogicManager { get; set; }

    protected BusinessLogicTestsBase(Action<ContainerBuilder>? registerTypes = null) : base(registerTypes)
    {
        BusinessLogicManager = Scope.Resolve<ITgBusinessLogicManager>();
    }

    #endregion
}
