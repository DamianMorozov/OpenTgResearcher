// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Helpers;

/// <summary> Disposable base class </summary>
public abstract class TgDisposable : ObservableRecipient, ITgDisposable
{
	#region Fields, properties, constructor

	public TgDisposable()
	{
		//
	}

    #endregion

    #region IDisposable

    /// <summary> Locker object </summary>
    protected Lock Locker { get; } = new();
    /// <summary> To detect redundant calls </summary>
    private bool _disposed;

    /// <summary> Finalizer </summary>
	~TgDisposable() => Dispose(false);

    /// <summary> Throw exception if disposed </summary>
    public void CheckIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary> Release managed resources </summary>
    public virtual void ReleaseManagedResources() { }

    /// <summary> Release unmanaged resources </summary>
    public virtual void ReleaseUnmanagedResources() { }

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
}
