namespace TgInfrastructure.Dtos;

public sealed class TgListWithUidDto
{
    public Guid Uid { get; set; } = default!;
    public string PrintString { get; set; } = default!;

    public TgListWithUidDto()
    {
        Uid = Guid.Empty;
        PrintString = string.Empty;
    }

    public TgListWithUidDto(string printString) : this()
    {
        PrintString = printString;
    }

    public TgListWithUidDto(Guid uid, string printString)
    {
        Uid = uid;
        PrintString = printString;
    }
}
