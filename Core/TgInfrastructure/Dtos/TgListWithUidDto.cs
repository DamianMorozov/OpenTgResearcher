// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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
