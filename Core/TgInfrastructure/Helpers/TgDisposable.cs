// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Helpers;

/// <summary> Disposable base class </summary>
public abstract class TgDisposable : ObservableRecipient, ITgDisposable
{
	#region Public and private fields, properties, constructor

	public TgDisposable()
	{
		//
	}

    #endregion

    #region IDisposable

    /// <summary> Locker object </summary>
    public object Locker { get; } = new();
    /// <summary> To detect redundant calls </summary>
    private bool _disposed;

    /// <summary> Finalizer </summary>
	~TgDisposable() => Dispose(false);

    /// <summary> Throw exception if disposed </summary>
    public void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException($"{nameof(TgDisposable)}: {TgConstants.ObjectHasBeenDisposedOff}!");
    }

    /// <summary> Release managed resources </summary>
    public virtual void ReleaseManagedResources() =>
        throw new NotImplementedException(TgConstants.UseOverrideMethod);

    /// <summary> Release unmanaged resources </summary>
    public virtual void ReleaseUnmanagedResources() =>
        throw new NotImplementedException(TgConstants.UseOverrideMethod);

    /// <summary> Dispose pattern </summary>
    public void Dispose()
    {
        // Dispose of unmanaged resources
        Dispose(true);
        // Suppress finalization
        GC.SuppressFinalize(this);
    }

    /// <summary> Dispose pattern </summary>
    protected void Dispose(bool disposing)
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
}
