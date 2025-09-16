namespace OpenTgResearcherDesktop.Contracts.Services;

public interface IAppNotificationService
{
    void Initialize();

    bool Show(string payload);

    NameValueCollection ParseArguments(string arguments);

    void Unregister();

    bool IsClientConnected { get; set; }

    event EventHandler<bool> ClientConnectionChanged;
}
