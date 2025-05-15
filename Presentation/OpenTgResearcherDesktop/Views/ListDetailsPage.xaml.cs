// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class ListDetailsPage
{
	#region Public and private fields, properties, constructor

	public ListDetailsViewModel ViewModel { get; }

	public ListDetailsPage()
	{
		ViewModel = App.GetService<ListDetailsViewModel>();
		InitializeComponent();
	}

	#endregion

	#region Public and private methods

	private void OnViewStateChanged(object sender, ListDetailsViewState e)
	{
		if (e == ListDetailsViewState.Both)
		{
			ViewModel.EnsureItemSelected();
		}
	}

	#endregion
}
