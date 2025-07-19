// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgMainPage
{
	#region Public and private fields, properties, constructor

	public override TgMainViewModel ViewModel { get; }

	public TgMainPage()
	{
		ViewModel = App.GetService<TgMainViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
