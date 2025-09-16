namespace TgAssertCoreTests.Helpers;

/// <summary>
/// Enumeration of values.
/// </summary>
public static class TgDataEnums
{
	/// <summary>
	/// Enumerable of bool values.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<bool> GetBool()
	{
		yield return false;
        yield return true;
	}

	/// <summary>
	/// Enumerable of bool values with null value.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<bool?> GetBoolNullable()
    {
        yield return null;
        yield return false;
        yield return true;
	}

	/// <summary>
	/// Enumerable of string values.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<string?> GetString()
	{
        yield return null;
        yield return "";
        yield return string.Empty;
	}

	/// <summary>
	/// Enumerable of Guid values.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<Guid> GetGuid()
    {
        yield return Guid.Empty;
        yield return Guid.NewGuid();
        yield return Guid.NewGuid();
        yield return Guid.NewGuid();
        yield return Guid.NewGuid();
	}

	/// <summary>
	/// Enumerable of decimal values.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<decimal> GetDecimal()
    {
        yield return decimal.MinValue;
        yield return decimal.MinValue / 2;
        yield return 0;
        yield return 1;
        yield return decimal.MaxValue / 2;
        yield return decimal.MaxValue;
	}

	/// <summary>
	/// Enumerable of ushort values.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<ushort> GetUshort()
    {
        yield return ushort.MinValue;
        yield return 1;
        yield return ushort.MaxValue / 2;
        yield return ushort.MaxValue;
	}

	/// <summary>
	/// Enumerable of progress values.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<ushort> GetProgress()
    {
        yield return 0;
        yield return 5;
        yield return 10;
        yield return 15;
        yield return 20;
        yield return 25;
        yield return 30;
        yield return 35;
        yield return 40;
        yield return 45;
        yield return 50;
        yield return 55;
        yield return 60;
        yield return 65;
        yield return 70;
        yield return 75;
        yield return 80;
        yield return 85;
        yield return 90;
        yield return 95;
        yield return 100;
	}

	/// <summary>
	/// Enumerable of short values.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<short> GetShort()
    {
        yield return short.MinValue;
        yield return 1;
        yield return short.MaxValue / 2;
        yield return short.MaxValue;
	}

	/// <summary>
	/// Enumerable of uint values.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<uint> GetUint()
    {
        yield return uint.MinValue;
        yield return 1;
        yield return uint.MaxValue / 2;
        yield return uint.MaxValue;
	}

	/// <summary>
	/// Enumerable of int values.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<int> GetInt()
    {
        yield return int.MinValue;
        yield return 1;
        yield return int.MaxValue / 2;
        yield return int.MaxValue;
	}

	/// <summary>
	/// Enumerable of int values with null value.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<int?> GetIntNullable()
    {
        yield return null;
        yield return int.MinValue;
        yield return 1;
        yield return int.MaxValue / 2;
        yield return int.MaxValue;
	}

	/// <summary>
	/// Enumerable of int values with null value.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<long?> GetLongNullable()
    {
        yield return null;
        yield return long.MinValue;
        yield return 1;
        yield return long.MaxValue / 2;
        yield return long.MaxValue;
	}

	/// <summary>
	/// Enumerable of long values.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<long> GetLong()
	{
        yield return long.MinValue;
        yield return 1;
        yield return long.MaxValue / 2;
        yield return long.MaxValue;
	}

	/// <summary>
	/// Enumerable of DateTime values.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<DateTime> GetDateTime()
	{
        yield return DateTime.MinValue;
        yield return DateTime.MaxValue;
        yield return DateTime.Now;
        yield return DateTime.Today;
        yield return DateTime.UtcNow;
	}

	/// <summary>
	/// String value.
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public static string AsString(this string str)
	{
		return str is null ? "<null>" : str == "" ? "<empty>" : str;
	}

	/// <summary>
	/// Enumerable of uri values.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<Uri> GetUri()
	{
        yield return new("http://google.com/");
        yield return new("http://microsoft.com/");
	}

	/// <summary>
	/// Enumerable of timeout values in ms.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<int> GetTimeoutMs()
    {
        yield return 50;
        yield return 500;
	}

	/// <summary>
	/// Enumerable of bytes.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<byte> GetBytes()
    {
        yield return 0x00;
        yield return 0x01;
        yield return 0x02;
        yield return 0x03;
        yield return 0x04;
        yield return 0x05;
        yield return 0x06;
        yield return 0x07;
        yield return 0x08;
        yield return 0x09;
        yield return 0x10;
	}
}
