namespace TgInfrastructure.Contracts;

public interface ITgDisposable : IDisposable
{
    /// <summary> Throw exception if disposed </summary>
    public void CheckIfDisposed();
    /// <summary> Release managed resources </summary>
    public void ReleaseManagedResources();
    /// <summary> Release unmanaged resources </summary>
    public void ReleaseUnmanagedResources();
}
