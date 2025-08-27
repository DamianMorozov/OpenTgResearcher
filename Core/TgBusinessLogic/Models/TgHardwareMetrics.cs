// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Models;

public record struct TgHardwareMetrics(
    DateTime TimestampUtc,
    double CpuAppPercent,
    double CpuTotalPercent,
    double MemoryAppPercent,
    double MemoryTotalPercent
);
