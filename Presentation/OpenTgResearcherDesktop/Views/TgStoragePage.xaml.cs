// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public partial class TgStoragePage
{
	#region Public and private fields, properties, constructor

	public override TgStorageViewModel ViewModel { get; }

	public TgStoragePage()
	{
		ViewModel = App.GetService<TgStorageViewModel>();
		InitializeComponent();
		Loaded += PageLoaded;
	}

	#endregion
}
