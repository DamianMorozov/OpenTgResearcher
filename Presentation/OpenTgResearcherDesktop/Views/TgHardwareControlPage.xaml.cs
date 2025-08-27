// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgHardwareControlPage
{
	#region Fields, properties, constructor

	public override TgHardwareControlViewModel ViewModel { get; }

	public TgHardwareControlPage()
	{
		ViewModel = App.GetService<TgHardwareControlViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
