// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain;

/// <summary> Blazor DB context </summary>
public sealed class TgEfBlazorContext : TgEfContextBase, ITgEfContext
{
    #region Public and private methods

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
		TgGlobalTools.SetAppType(TgEnumAppType.Blazor);
		optionsBuilder.UseSqlite(GetStoragePath(TgGlobalTools.AppType));
    }

	#endregion
}