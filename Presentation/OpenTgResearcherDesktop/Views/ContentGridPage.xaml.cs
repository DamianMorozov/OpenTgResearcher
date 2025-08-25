// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class ContentGridPage
{
	#region Fields, properties, constructor

	public ContentGridViewModel ViewModel { get; }

	public ContentGridPage()
	{
		ViewModel = App.GetService<ContentGridViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
    }

    #endregion
}
