namespace OpenTgResearcherDesktop.Models;

/// <summary> Неизменяемый снимок доступных показаний для временного графика </summary>
public sealed record TgHardwareSample(DateTime TimestampUtc, double? CpuApp, double? CpuTotal, double? MemoryApp, double? MemoryTotal);
