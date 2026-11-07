namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Нативное отображение сохранённой разметки Telegram без исполнения HTML </summary>
public static class TgMessageTextHelper
{
    public static readonly DependencyProperty ContentProperty = DependencyProperty.RegisterAttached("Content", typeof(TgMessageContent),
        typeof(TgMessageTextHelper), new PropertyMetadata(null, OnContentChanged));

    /// <summary> Возвращает исходный текст с разметкой </summary>
    public static TgMessageContent? GetContent(DependencyObject element) => element.GetValue(ContentProperty) as TgMessageContent;

    /// <summary> Устанавливает исходный текст с разметкой </summary>
    public static void SetContent(DependencyObject element, TgMessageContent? value) => element.SetValue(ContentProperty, value);

    // Собираем фрагменты по границам диапазонов, сохраняя вложенные стили и переносы строк
    private static void OnContentChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
    {
        if (element is not RichTextBlock block) return;
        block.Blocks.Clear();
        if (args.NewValue is not TgMessageContent content) return;
        var paragraph = new Microsoft.UI.Xaml.Documents.Paragraph();
        foreach (var part in content.GetRuns())
        {
            var run = new Microsoft.UI.Xaml.Documents.Run { Text = part.Text };
            if (part.Bold) run.FontWeight = Microsoft.UI.Text.FontWeights.Bold;
            if (part.Italic) run.FontStyle = Windows.UI.Text.FontStyle.Italic;
            if (part.Monospace) run.FontFamily = new FontFamily("Consolas");
            run.TextDecorations = (part.Underline ? Windows.UI.Text.TextDecorations.Underline : Windows.UI.Text.TextDecorations.None)
                | (part.Strike ? Windows.UI.Text.TextDecorations.Strikethrough : Windows.UI.Text.TextDecorations.None);
            if (Uri.TryCreate(part.Link, UriKind.Absolute, out var uri))
            {
                var link = new Microsoft.UI.Xaml.Documents.Hyperlink { NavigateUri = uri };
                link.Inlines.Add(run); paragraph.Inlines.Add(link);
            }
            else paragraph.Inlines.Add(run);
        }
        block.Blocks.Add(paragraph);
    }
}
