//// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
//// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

//namespace TgStorageTest.Common;

//[TestFixture]
//internal sealed class TgEfTestEntityTests
//{
//	#region Methods

//	[Test]
//	public void Get_default_properties()
//	{
//		Assert.DoesNotThrow(() =>
//		{
//			TgEfTestEntity item = new();
//			Assert.Multiple(() =>
//			{
//				Assert.That(Equals(Guid.Empty, item.Uid));
//				Assert.That(Equals(true, item.BoolValue));
//				Assert.That(Equals(TgEnumCommon.Some1, item.EnumValue));
//				Assert.That(Equals("Some string", item.StringValue));
//				Assert.That(Equals((short)-101, item.ShortValue));
//				Assert.That(Equals((ushort)101, item.UshortValue));
//				Assert.That(Equals((int)-201, item.IntValue));
//				Assert.That(Equals((uint)201, item.UintValue));
//				Assert.That(Equals((long)-301, item.LongValue));
//				Assert.That(Equals((ulong)301, item.UlongValue));
//				Assert.That(DateTime.ParseExact("2001-02-03 11:22:33", "yyyy-MM-dd HH:mm:ss",
//					System.Globalization.CultureInfo.InvariantCulture), Is.EqualTo(item.DtValue));
//			});
//		});
//	}

//	#endregion
//}