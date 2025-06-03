// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Dtos;

public sealed class TgLicenseFeatureDto
{
    public string Feature { get; set; } = string.Empty;
    public string Free { get; set; } = string.Empty;
    public string Test { get; set; } = string.Empty;
    public string Paid { get; set; } = string.Empty;
    public string Premium { get; set; } = string.Empty;
}