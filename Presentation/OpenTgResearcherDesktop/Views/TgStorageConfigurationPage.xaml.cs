// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public partial class TgStorageConfigurationPage
{
	#region Fields, properties, constructor

	public override TgStorageConfigurationViewModel ViewModel { get; }

	public TgStorageConfigurationPage()
	{
		ViewModel = App.GetService<TgStorageConfigurationViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
