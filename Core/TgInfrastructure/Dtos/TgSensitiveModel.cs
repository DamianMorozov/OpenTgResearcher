// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Dtos;

/// <summary> Sensitive model </summary>
[DebuggerDisplay("{ToDebugString()}")]
public partial class TgSensitiveModel : ObservableRecipient
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial string SensitiveData { get; set; } = "**********";
    [ObservableProperty]
    public partial bool IsDisplaySensitiveData { get; set; } = true;

    public IRelayCommand? SetDisplaySensitiveCommand { get; set; }

    public TgSensitiveModel() : base() { }

    #endregion

    #region Public and private methods

    partial void OnIsDisplaySensitiveDataChanged(bool value)
    {
        if (SetDisplaySensitiveCommand?.CanExecute(value) ?? false)
            SetDisplaySensitiveCommand.Execute(value);
    }

    #endregion
}
