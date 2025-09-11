// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Converters;

/// <summary> Mapping between enum value and object </summary>
public sealed class TgEnumToObjectMapping
{
    public string EnumValue { get; set; } = string.Empty;
    public object ObjectValue { get; set; } = string.Empty;
}
