// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class ContentGridDetailPage
{
	#region Fields, properties, constructor

	public ContentGridDetailViewModel ViewModel { get; }

	public ContentGridDetailPage()
	{
		ViewModel = App.GetService<ContentGridDetailViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
    }

    #endregion

    #region Methods

    protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		try
		{
			base.OnNavigatedTo(e);
			this.RegisterElementForConnectedAnimation("animationKeyContentGrid", itemHero);
		}
		catch (Exception ex)
		{
			TgLogUtils.WriteExceptionWithMessage(ex, "An error occurred during navigation!");
		}
	}

	protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
	{
		base.OnNavigatingFrom(e);
		if (e.NavigationMode == NavigationMode.Back)
		{
			var navigationService = App.GetService<INavigationService>();

			if (ViewModel.Item != null)
			{
				navigationService.SetListDataItemForNextConnectedAnimation(ViewModel.Item);
			}
		}
	}

	#endregion
}
