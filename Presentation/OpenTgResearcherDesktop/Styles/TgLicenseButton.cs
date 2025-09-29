namespace OpenTgResearcherDesktop.Styles;

/// <summary> Button to show license acquisition dialog </summary>
public sealed partial class TgLicenseButton : Button
{
    #region Fields, properties, constructor

    public IAsyncRelayCommand? ShowPurchaseLicenseCommand
    {
        get => (IAsyncRelayCommand?)GetValue(ShowRequiredLicenseCommandProperty);
        set => SetValue(ShowRequiredLicenseCommandProperty, value);
    }
    public static readonly DependencyProperty ShowRequiredLicenseCommandProperty =
        DependencyProperty.Register(nameof(ShowPurchaseLicenseCommand), typeof(IAsyncRelayCommand), typeof(TgLicenseButton),
            new PropertyMetadata(null));

    public TgLicenseButton()
    {
        DefaultStyleKey = typeof(TgLicenseButton);
        Click += OnClick;
    }

    #endregion

    #region Methods

    private void OnClick(object sender, RoutedEventArgs e)
    {
        bool isLicensed = TgDesktopUtils.VerifyLicense();

        if (!isLicensed)
        {
            Command = null;
            CommandParameter = null;
            ShowPurchaseLicenseCommand?.Execute(null);
        }
        // If there is a license, event processing continues, for example, calling Click
    }

    #endregion
}
