// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Apps;

/// <summary> App ViewModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfAppViewModel : TgEntityViewModelBase<TgEfAppEntity, TgEfAppDto>, ITgDtoViewModel, IDisposable
{
    #region Fields, properties, constructor

    public override ITgEfAppRepository Repository { get; }
    [ObservableProperty]
    public partial TgEfAppDto Dto { get; set; } = null!;

    public TgEfAppViewModel(Autofac.IContainer container, TgEfAppEntity item) : base()
    {
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfAppRepository>();
        Fill(item);
    }

    public TgEfAppViewModel(Autofac.IContainer container) : base()
    {
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfAppRepository>();
        TgEfAppEntity item = new();
        Fill(item);
    }

    #endregion

    #region IDisposable

    /// <summary> Locker object </summary>
    private Lock Locker { get; } = new();
    /// <summary> To detect redundant calls </summary>
    private bool _disposed;

    /// <summary> Finalizer </summary>
	~TgEfAppViewModel() => Dispose(false);

    /// <summary> Throw exception if disposed </summary>
    public void CheckIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

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
        using (Locker.EnterScope())
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

    #region Methods

    public override string ToString() => Dto.ToString();

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