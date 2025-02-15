// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain;

/// <summary> Desktop DB context </summary>
public sealed class TgEfDesktopContext : TgEfContextBase, ITgEfContext
{
    #region Public and private methods

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
	    base.OnConfiguring(optionsBuilder);
		TgGlobalTools.SetAppType(TgEnumAppType.Desktop);
		optionsBuilder.UseSqlite(GetStoragePath(TgGlobalTools.AppType));
    }

	#endregion
}