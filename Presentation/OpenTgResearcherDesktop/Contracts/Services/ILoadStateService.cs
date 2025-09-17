using System.ComponentModel;

namespace OpenTgResearcherDesktop.Contracts.Services;

public interface ILoadStateService : INotifyPropertyChanged
{
    bool IsStorageProcessing { get; set; }
    bool IsOnlineProcessing { get; set; }
}
