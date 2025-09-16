namespace TgInfrastructure.Api;

public sealed class TgApiResult
{
	public bool IsOk { get; set; }
	public object Value { get; set; } = string.Empty;
}
