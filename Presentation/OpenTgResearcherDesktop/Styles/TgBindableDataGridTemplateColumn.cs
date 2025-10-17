namespace OpenTgResearcherDesktop.Styles;

/// <summary> Template column that swaps cell DataTemplate by boolean flag without hiding the column </summary>
public sealed partial class TgBindableDataGridTemplateColumn : DataGridTemplateColumn
{
    /// <summary> OwnerGrid reference for forcing refresh </summary>
    public DataGrid? OwnerGrid
    {
        get => (DataGrid?)GetValue(OwnerGridProperty);
        set => SetValue(OwnerGridProperty, value);
    }

    /// <summary> OwnerGrid reference for forcing refresh </summary>
    public static readonly DependencyProperty OwnerGridProperty = DependencyProperty.Register(nameof(OwnerGrid), typeof(DataGrid),
            typeof(TgBindableDataGridTemplateColumn), new PropertyMetadata(null));

    /// <summary> ColumnWidth </summary>
    public DataGridLength ColumnWidth
    {
        get => (DataGridLength)GetValue(ColumnWidthProperty);
        set => SetValue(ColumnWidthProperty, value);
    }

    private static void OnColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var col = (TgBindableDataGridTemplateColumn)d;
        col.Width = (DataGridLength)e.NewValue;
    }

    /// <summary> ColumnWidth </summary>
    public static readonly DependencyProperty ColumnWidthProperty = DependencyProperty.Register(nameof(ColumnWidth), typeof(DataGridLength),
            typeof(TgBindableDataGridTemplateColumn), new PropertyMetadata(new DataGridLength(1, DataGridLengthUnitType.Auto), OnColumnWidthChanged));

    /// <summary> ColumnValue (original template) </summary>
    public DataTemplate ColumnValue
    {
        get => (DataTemplate)GetValue(ColumnValueProperty);
        set => SetValue(ColumnValueProperty, value);
    }

    /// <summary> ColumnValue (original template) </summary>
    public static readonly DependencyProperty ColumnValueProperty = DependencyProperty.Register(nameof(ColumnValue), typeof(DataTemplate),
            typeof(TgBindableDataGridTemplateColumn), new PropertyMetadata(null, OnAnyTemplateChanged));

    /// <summary> ColumnSensitive (masked template) </summary>
    public DataTemplate ColumnSensitive
    {
        get => (DataTemplate)GetValue(ColumnSensitiveProperty);
        set => SetValue(ColumnSensitiveProperty, value);
    }

    /// <summary> ColumnSensitive (masked template) </summary>
    public static readonly DependencyProperty ColumnSensitiveProperty = DependencyProperty.Register(nameof(ColumnSensitive), typeof(DataTemplate),
            typeof(TgBindableDataGridTemplateColumn), new PropertyMetadata(null, OnAnyTemplateChanged));

    /// <summary> ColumnVisible (true → show original, false → show masked) </summary>
    public bool ColumnVisible
    {
        get => (bool)GetValue(ColumnVisibleProperty);
        set => SetValue(ColumnVisibleProperty, value);
    }

    private static void OnColumnVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => 
        UpdateCellTemplate((TgBindableDataGridTemplateColumn)d);

    /// <summary> ColumnVisible (true → show original, false → show masked) </summary>
    public static readonly DependencyProperty ColumnVisibleProperty = DependencyProperty.Register(nameof(ColumnVisible), typeof(bool),
        typeof(TgBindableDataGridTemplateColumn), new PropertyMetadata(true, OnColumnVisibleChanged));

    private static void OnAnyTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => 
        UpdateCellTemplate((TgBindableDataGridTemplateColumn)d);

    /// <summary> Updates the cell template based on ColumnVisible and forces DataGrid to recreate the column </summary>
    private static void UpdateCellTemplate(TgBindableDataGridTemplateColumn col) => TgDesktopUtils.InvokeOnUIThread(() =>
    {
        // Select template based on ColumnVisible flag
        var newTemplate = col.ColumnVisible ? col.ColumnValue : col.ColumnSensitive;
        if (newTemplate is null)
            return;

        // Assign new template
        col.CellTemplate = newTemplate;

        // Force DataGrid to recreate the column
        if (col.OwnerGrid is DataGrid grid)
        {
            int index = grid.Columns.IndexOf(col);
            if (index >= 0)
            {
                grid.Columns.RemoveAt(index);
                grid.Columns.Insert(index, col);
            }

            grid.UpdateLayout();
        }
    });
}
