// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

// TODO: Change the grid as appropriate for your app. Adjust the column definitions on DataGridPage.xaml.
// For more details, see the documentation at https://docs.microsoft.com/windows/communitytoolkit/controls/datagrid.
public sealed partial class DataGridPage
{
	#region Public and private fields, properties, constructor

	public DataGridViewModel ViewModel { get; }

	public DataGridPage()
	{
		ViewModel = App.GetService<DataGridViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
    }

    #endregion
}
