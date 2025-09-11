// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com


namespace OpenTgResearcherDesktop.Styles;

/// <summary> Button to show license acquisition dialog </summary>
public sealed partial class TgLicenseButton : Button
{
    #region Fields, properties, constructor

    public IRelayCommand? ShowPurchaseLicenseCommand
    {
        get => (IRelayCommand?)GetValue(ShowRequiredLicenseCommandProperty);
        set => SetValue(ShowRequiredLicenseCommandProperty, value);
    }
    public static readonly DependencyProperty ShowRequiredLicenseCommandProperty =
        DependencyProperty.Register(nameof(ShowPurchaseLicenseCommand), typeof(IRelayCommand), typeof(TgLicenseButton),
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
        bool isLicensed = TgDesktopUtils.VerifyPaidLicense();

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
