// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatsPage
{
	#region Public and private fields, properties, constructor

	public override TgChatsViewModel ViewModel { get; }

	public TgChatsPage()
	{
		ViewModel = App.GetService<TgChatsViewModel>();
		InitializeComponent();
		Loaded += PageLoaded;
	}

	#endregion

	#region Public and private methods

	//

	#endregion
}
