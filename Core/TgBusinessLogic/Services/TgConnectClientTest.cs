// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com


// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com


namespace TgBusinessLogic.Services;

/// <summary> Test connection client </summary>
public sealed partial class TgConnectClientTest : TgConnectClientBase, ITgConnectClientTest
{
    public TgConnectClientTest(TgStorageManager storageManager) : base(storageManager)
    {
        //
    }
}
