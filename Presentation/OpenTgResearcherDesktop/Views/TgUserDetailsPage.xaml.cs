// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgUserDetailsPage
{
	#region Public and private fields, properties, constructor

	public override TgUserDetailsViewModel ViewModel { get; }

	public TgUserDetailsPage()
	{
		ViewModel = App.GetService<TgUserDetailsViewModel>();

		InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

    #endregion
}
