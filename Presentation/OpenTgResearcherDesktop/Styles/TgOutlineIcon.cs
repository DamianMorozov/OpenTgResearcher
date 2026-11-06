namespace OpenTgResearcherDesktop.Styles;

/// <summary> Масштабируемая контурная иконка Lucide, учитывающая тему приложения </summary>
public sealed class TgOutlineIcon : UserControl
{
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(nameof(Kind), typeof(string), typeof(TgOutlineIcon),
        new PropertyMetadata("hardware", OnKindChanged));
    private readonly Image _image = new() { Stretch = Stretch.Uniform };

    public string Kind { get => (string)GetValue(KindProperty); set => SetValue(KindProperty, value); }

    /// <summary> Создаёт иконку без отдельной остановки клавиатурного фокуса </summary>
    public TgOutlineIcon()
    {
        Width = 20;
        Height = 20;
        IsTabStop = false;
        IsHitTestVisible = false;
        Content = _image;
        Loaded += OnLoaded;
        ActualThemeChanged += OnThemeChanged;
    }

    // Загружаем ресурс после определения действующей темы окна
    private void OnLoaded(object sender, RoutedEventArgs e) => UpdateSource();

    // Меняем набор контуров при переключении оформления
    private void OnThemeChanged(FrameworkElement sender, object args) => UpdateSource();

    // Обновляем иконку при смене состояния кнопки
    private static void OnKindChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) => ((TgOutlineIcon)sender).UpdateSource();

    // Подбираем контрастный контур и оставляем лог при ошибке ресурса
    private void UpdateSource()
    {
        try
        {
            var theme = ActualTheme == ElementTheme.Dark ? "dark" : "light";
            _image.Source = new SvgImageSource(new Uri($"ms-appx:///Assets/Icons/Lucide/{Kind}-{theme}.svg"));
        }
        catch (Exception ex) { TgLogUtils.WriteException(ex, "Не удалось загрузить контурную иконку"); }
    }
}
