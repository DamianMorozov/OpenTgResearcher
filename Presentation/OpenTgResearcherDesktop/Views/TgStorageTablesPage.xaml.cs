// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public partial class TgStorageTablesPage
{
	#region Fields, properties, constructor

	public override TgStorageTablesViewModel ViewModel { get; }

	public TgStorageTablesPage()
	{
		ViewModel = App.GetService<TgStorageTablesViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

    #endregion
}
