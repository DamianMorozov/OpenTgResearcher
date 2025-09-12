namespace TgInfrastructure.Dtos;

/// <summary> Sensitive model </summary>
[DebuggerDisplay("{ToDebugString()}")]
public partial class TgSensitiveModel : ObservableRecipient
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial string SensitiveData { get; set; } = "**********";
    [ObservableProperty]
    public partial bool IsDisplaySensitiveData { get; set; } = true;

    public IRelayCommand? SetDisplaySensitiveCommand { get; set; }

    public TgSensitiveModel() : base()
    {
        SetDisplaySensitiveCommand = new AsyncRelayCommand(SetDisplaySensitiveAsync);
    }

    #endregion

    #region Methods

    /// <summary> Action for IsDisplaySensitiveData property change </summary>
    partial void OnIsDisplaySensitiveDataChanged(bool value)
    {
        if (SetDisplaySensitiveCommand?.CanExecute(value) ?? false)
            SetDisplaySensitiveCommand.Execute(value);
    }

    protected virtual async Task SetDisplaySensitiveAsync() => await Task.CompletedTask;

    #endregion
}
