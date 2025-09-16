namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgLoadDataPage
{
    #region Fields, properties, constructor

    public override TgLoadDataViewModel ViewModel { get; }
    public bool IsPageLoad { get => field; set { field = value; ViewModel.IsPageLoad = field;  OnPropertyChanged(); } }
    public bool IsOnlineProcessing { get => field; set { field = value; ViewModel.IsOnlineProcessing = field; OnPropertyChanged(); } }

    public TgLoadDataPage()
    {
        ViewModel = App.GetService<TgLoadDataViewModel>();

        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
    }

    #endregion
}
