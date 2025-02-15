// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderBlazor.Common;

public class TgLayoutComponentBase : LayoutComponentBase
{
    #region Public and private fields, properties, constructor

    [Inject] protected NavigationManager UriHelper { get; set; } = null!;

    #endregion
}