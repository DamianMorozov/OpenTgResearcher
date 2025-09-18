using System.ComponentModel;

namespace OpenTgResearcherDesktop.Contracts.Services;

/// <summary> Load state service to track loading states in the application </summary>
public interface ILoadStateService : INotifyPropertyChanged
{
    bool IsStorageProcessing { get; set; }
    bool IsOnlineProcessing { get; set; }
    bool IsDisplaySensitiveData { get; set; }
    string SensitiveData { get; }
}
