// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgUpdatePage
{
	#region Public and private fields, properties, constructor

	public override TgUpdateViewModel ViewModel { get; }

	public TgUpdatePage()
	{
		ViewModel = App.GetService<TgUpdateViewModel>();
		InitializeComponent();
		Loaded += PageLoaded;
	}

	#endregion
}
