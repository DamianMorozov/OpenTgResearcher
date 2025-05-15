// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgLoadDataPage
{
	#region Public and private fields, properties, constructor

	public override TgLoadDataViewModel ViewModel { get; }

	public TgLoadDataPage()
	{
		ViewModel = App.GetService<TgLoadDataViewModel>();
		InitializeComponent();
		Loaded += PageLoaded;
	}

	#endregion
}
