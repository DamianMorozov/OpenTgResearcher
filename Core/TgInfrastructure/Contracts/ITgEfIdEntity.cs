// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Contracts;

/// <summary> SQL entity </summary>
public interface ITgEfIdEntity<TEfEntity> : ITgEfEntity<TEfEntity>
	where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
{
    #region Public and private fields, properties, constructor

    public long Id { get; set; }

    #endregion
}
