// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgFiltersPage
{
	#region Fields, properties, constructor

	public override TgFiltersViewModel ViewModel { get; }

	public TgFiltersPage()
	{
		ViewModel = App.GetService<TgFiltersViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
