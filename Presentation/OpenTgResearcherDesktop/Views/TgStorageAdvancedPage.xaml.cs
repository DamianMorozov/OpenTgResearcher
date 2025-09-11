// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public partial class TgStorageAdvancedPage
{
	#region Fields, properties, constructor

	public override TgStorageAdvancedViewModel ViewModel { get; }

	public TgStorageAdvancedPage()
	{
		ViewModel = App.GetService<TgStorageAdvancedViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
