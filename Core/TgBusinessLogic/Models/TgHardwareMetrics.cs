namespace TgBusinessLogic.Models;

public record struct TgHardwareMetrics(
    DateTime TimestampUtc,
    double CpuAppPercent,
    double CpuTotalPercent,
    double MemoryAppPercent,
    double MemoryTotalPercent
);
