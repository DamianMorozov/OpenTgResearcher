﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Contracts;

public interface ITgEfVersionRepository : ITgEfRepository<TgEfVersionEntity, TgEfVersionDto>, IDisposable
{
    public short LastVersion { get; }
	public Task<TgEfVersionEntity> GetLastVersionAsync();
    public Task FillTableVersionsAsync();
}