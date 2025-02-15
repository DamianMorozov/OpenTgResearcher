// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain;

/// <summary> Test DB context </summary>
public sealed class TgEfTestContext : TgEfContextBase, ITgEfContext
{
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);
		TgGlobalTools.SetAppType(TgEnumAppType.Test);
		optionsBuilder.UseSqlite(GetStoragePath(TgGlobalTools.AppType));
	}
}
