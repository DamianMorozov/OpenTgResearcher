// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class ListDetailsDetailControl
{
	#region Fields, properties, constructor

	public SampleOrder? ListDetailsMenuItem
	{
		get => GetValue(ListDetailsMenuItemProperty) as SampleOrder;
		set => SetValue(ListDetailsMenuItemProperty, value);
	}

	public static readonly DependencyProperty ListDetailsMenuItemProperty = 
		DependencyProperty.Register("ListDetailsMenuItem", typeof(SampleOrder), typeof(ListDetailsDetailControl), new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

	public ListDetailsDetailControl()
	{
		InitializeComponent();
	}

	#endregion

	#region Methods

	private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is ListDetailsDetailControl control)
		{
			control.ForegroundElement.ChangeView(0, 0, 1);
		}
	}

	#endregion
}
