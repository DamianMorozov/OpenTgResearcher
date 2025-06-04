// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Tests.MSTest;

// TODO: Write unit tests.
// https://docs.microsoft.com/visualstudio/test/getting-started-with-unit-testing
// https://docs.microsoft.com/visualstudio/test/using-microsoft-visualstudio-testtools-unittesting-members-in-unit-tests
// https://docs.microsoft.com/visualstudio/test/run-unit-tests-with-test-explorer

[TestClass]
public class TestClass
{
	[ClassInitialize]
	public static void ClassInitialize(TestContext context)
	{
		Debug.WriteLine("  ClassInitialize");
	}

	[ClassCleanup]
	public static void ClassCleanup()
	{
		Debug.WriteLine("  ClassCleanup");
	}

	[TestInitialize]
	public void TestInitialize()
	{
		Debug.WriteLine("  TestInitialize");
	}

	[TestCleanup]
	public void TestCleanup()
	{
		Debug.WriteLine("  TestCleanup");
	}

	[TestMethod]
	public void TestMethod()
	{
		Assert.IsTrue(true);
	}

	//[UITestMethod]
	//public void UITestMethod()
	//{
	//	Assert.AreEqual(0, new Grid().ActualWidth);
	//}
}
