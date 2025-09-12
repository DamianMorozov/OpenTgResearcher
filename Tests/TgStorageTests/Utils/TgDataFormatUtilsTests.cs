namespace TgStorageTests.Utils;

[TestFixture]
public class TgDataFormatUtilsTests
{
	#region Methods

	[Test]
	public void DataFormatUtils_CheckFileAtMask_AreEqual()
	{
		Assert.DoesNotThrow(() =>
		{
			var fileName = "RW-Kotlin-Cheatsheet-1.1.pdf";
			var result = TgDataFormatUtils.CheckFileAtMask(fileName, "kotlin");
			Assert.That(result);
			result = TgDataFormatUtils.CheckFileAtMask(fileName, "PDF");
			Assert.That(result);

			fileName = "C# Generics.ZIP";
			result = TgDataFormatUtils.CheckFileAtMask(fileName, "c*#");
			Assert.That(result);
			result = TgDataFormatUtils.CheckFileAtMask(fileName, "zip");
			Assert.That(result);
		});
	}

	#endregion
}
