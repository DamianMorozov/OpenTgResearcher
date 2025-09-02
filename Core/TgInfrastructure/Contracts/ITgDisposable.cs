// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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
