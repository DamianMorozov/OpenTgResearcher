//using Microsoft.UI.Xaml.Markup;

namespace OpenTgResearcherDesktop.Styles;

//[ContentProperty(Name = nameof(ColumnOriginValue))]
public sealed partial class TgTgBindableDataGridTemplateColumn : DataGridTemplateColumn
{
    //// 1. ColumnHeader (локализуемый через x:Uid="ResourceKey")
    //public static readonly DependencyProperty ColumnHeaderProperty =
    //    DependencyProperty.Register(
    //        nameof(ColumnHeader),
    //        typeof(string),
    //        typeof(TgBindableDataGridTemplateColumn),
    //        new PropertyMetadata(string.Empty, OnColumnHeaderChanged),
    //        // отмечаем локализуемым
    //        (d, v) => true);

    //public string ColumnHeader
    //{
    //    get => (string)GetValue(ColumnHeaderProperty);
    //    set => SetValue(ColumnHeaderProperty, value);
    //}

    //private static void OnColumnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{
    //    var col = (TgBindableDataGridTemplateColumn)d;
    //    col.Header = e.NewValue;
    //}

    //// 2. ColumnWidth
    //public static readonly DependencyProperty ColumnWidthProperty =
    //    DependencyProperty.Register(
    //        nameof(ColumnWidth),
    //        typeof(DataGridLength),
    //        typeof(TgBindableDataGridTemplateColumn),
    //        new PropertyMetadata(new DataGridLength(1, DataGridLengthUnitType.Auto), OnColumnWidthChanged));

    //public DataGridLength ColumnWidth
    //{
    //    get => (DataGridLength)GetValue(ColumnWidthProperty);
    //    set => SetValue(ColumnWidthProperty, value);
    //}

    //private static void OnColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{
    //    var col = (TgBindableDataGridTemplateColumn)d;
    //    col.Width = (DataGridLength)e.NewValue;
    //}

    //// 3. ColumnSensitiveValue
    //public static readonly DependencyProperty ColumnSensitiveValueProperty =
    //    DependencyProperty.Register(
    //        nameof(ColumnSensitiveValue),
    //        typeof(DataTemplate),
    //        typeof(TgBindableDataGridTemplateColumn),
    //        new PropertyMetadata(null, OnAnyTemplateChanged));

    //public DataTemplate ColumnSensitiveValue
    //{
    //    get => (DataTemplate)GetValue(ColumnSensitiveValueProperty);
    //    set => SetValue(ColumnSensitiveValueProperty, value);
    //}

    //// 4. ColumnOriginValue
    //public static readonly DependencyProperty ColumnOriginValueProperty =
    //    DependencyProperty.Register(
    //        nameof(ColumnOriginValue),
    //        typeof(DataTemplate),
    //        typeof(TgBindableDataGridTemplateColumn),
    //        new PropertyMetadata(null, OnAnyTemplateChanged));

    //public DataTemplate ColumnOriginValue
    //{
    //    get => (DataTemplate)GetValue(ColumnOriginValueProperty);
    //    set => SetValue(ColumnOriginValueProperty, value);
    //}

    //// 5. ColumnVisible
    //public static readonly DependencyProperty ColumnVisibleProperty =
    //    DependencyProperty.Register(
    //        nameof(ColumnVisible),
    //        typeof(bool),
    //        typeof(TgBindableDataGridTemplateColumn),
    //        new PropertyMetadata(true, OnColumnVisibleChanged));

    //public bool ColumnVisible
    //{
    //    get => (bool)GetValue(ColumnVisibleProperty);
    //    set => SetValue(ColumnVisibleProperty, value);
    //}

    //private static void OnColumnVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{
    //    UpdateCellTemplate((TgBindableDataGridTemplateColumn)d);
    //}

    //private static void OnAnyTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{
    //    UpdateCellTemplate((TgBindableDataGridTemplateColumn)d);
    //}

    //private static void UpdateCellTemplate(TgBindableDataGridTemplateColumn col)
    //{
    //    // выбираем шаблон в зависимости от ColumnVisible
    //    if (col.ColumnVisible)
    //        col.CellTemplate = col.ColumnOriginValue;
    //    else
    //        col.CellTemplate = col.ColumnSensitiveValue;
    //}
}
