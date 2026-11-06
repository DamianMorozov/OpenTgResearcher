namespace TgBusinessLogic.Models;

/// <summary> Назначение сетевой операции </summary>
public enum TgNetworkOperationKind { Request, Connection, Disconnection, Parsing, Download, Updates, Proxies }

/// <summary> Наблюдаемое состояние сетевой операции </summary>
public enum TgNetworkOperationState { Running, Completed, Canceled, Failed }

/// <summary> Снимок операции в памяти текущего запуска </summary>
public sealed record TgNetworkOperation(Guid Id, TgNetworkOperationKind Kind, DateTime StartedUtc,
    DateTime? FinishedUtc, TgNetworkOperationState State, string Detail = "", double? Progress = null,
    long TransferredBytes = 0, long TotalBytes = 0, bool CanCancel = false, bool IsCancellationRequested = false, bool IsPaused = false);
