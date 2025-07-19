// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Apps;

/// <summary> App view-model </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfAppViewModel : TgEntityViewModelBase<TgEfAppEntity, TgEfAppDto>, ITgDtoViewModel, IDisposable
{
    #region Public and private fields, properties, constructor

    public override TgEfAppRepository Repository { get; } = new();
    [ObservableProperty]
    public partial TgEfAppDto Dto { get; set; } = null!;

    public TgEfAppViewModel(TgEfAppEntity item) : base()
    {
        Fill(item);
    }

    public TgEfAppViewModel() : base()
    {
        TgEfAppEntity item = new();
        Fill(item);
    }

    #endregion

    #region IDisposable

    /// <summary> Locker object </summary>
    private object Locker { get; } = new();
    /// <summary> To detect redundant calls </summary>
    private bool _disposed;

    /// <summary> Finalizer </summary>
	~TgEfAppViewModel() => Dispose(false);

    /// <summary> Throw exception if disposed </summary>
    public void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException($"{nameof(TgEfAppViewModel)}: {TgConstants.ObjectHasBeenDisposedOff}!");
    }

    /// <summary> Release managed resources </summary>
    public void ReleaseManagedResources()
    {
        Repository.Dispose();
    }

    /// <summary> Release unmanaged resources </summary>
    public void ReleaseUnmanagedResources()
    {
        //
    }

    /// <summary> Dispose pattern </summary>
    public void Dispose()
    {
        // Dispose of unmanaged resources
        Dispose(true);
        // Suppress finalization
        GC.SuppressFinalize(this);
    }

    /// <summary> Dispose pattern </summary>
    public void Dispose(bool disposing)
    {
        if (_disposed) return;
        lock (Locker)
        {
            // Release managed resources
            if (disposing)
                ReleaseManagedResources();
            // Release unmanaged resources
            ReleaseUnmanagedResources();
            // Flag
            _disposed = true;
        }
    }

    #endregion

    #region Public and private methods

    public override string ToString() => Dto.ToString() ?? string.Empty;

    public override string ToDebugString() => Dto.ToDebugString();

    public void Fill(TgEfAppEntity item)
    {
        Dto ??= new();
        Dto.Copy(item, isUidCopy: true);
    }

    public async Task<TgEfStorageResult<TgEfAppEntity>> SaveAsync() =>
        await Repository.SaveAsync(Dto.GetNewEntity());

    #endregion
}