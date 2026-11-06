namespace OpenTgResearcherDesktop.HardwareSelfTests;

internal static class Program
{
    // Проверяем настоящую модель измерений и разметку без Telegram и пользовательской БД
    private static int Main()
    {
        var tests = new (string Name, Action Run)[]
        {
            ("InitialOverview", InitialOverview), ("MeasurementsAndHistory", MeasurementsAndHistory),
            ("PausePreservesReadings", PausePreservesReadings), ("MinuteWindowAndGaps", MinuteWindowAndGaps),
            ("UnavailableIsNotZero", UnavailableIsNotZero), ("ViewChangePreservesData", ViewChangePreservesData),
            ("HardwareLayout", HardwareLayout), ("ShellLayout", ShellLayout), ("Localization", Localization)
        };
        var failures = 0;
        foreach (var test in tests)
        {
            try { test.Run(); Console.WriteLine($"PASS: {test.Name}"); }
            catch (Exception ex) { failures++; Console.WriteLine($"FAIL: {test.Name}: {ex.GetBaseException().Message}"); }
        }
        return failures == 0 ? 0 : 1;
    }

    // Создаём исходную модель страницы, а не её тестовую копию
    private static object Create() => Activator.CreateInstance(Assembly.GetExecutingAssembly()
        .GetType("OpenTgResearcherDesktop.Models.TgHardwareDashboardModel") ?? throw new InvalidOperationException("Отсутствует модель истории оборудования"))!;

    // Читаем наблюдаемое состояние модели
    private static T Read<T>(object model, string name) => (T)model.GetType().GetProperty(name)!.GetValue(model)!;

    // Вызываем действие исходной модели
    private static void Call(object model, string name, params object[] args) => model.GetType().GetMethod(name)!.Invoke(model, args);

    // Создаём воспроизводимое измерение с независимым временем
    private static TgHardwareMetrics Sample(int second, double cpu = 6) => new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(second),
        cpu, 38, 2, 65, 0.328125, 10.4, 16);

    // Подсчитываем реальные точки истории
    private static object[] History(object model) => Read<IEnumerable>(model, "History").Cast<object>().ToArray();

    // Проверяем исходное отсутствие показаний и стартовый вид
    private static void InitialOverview()
    {
        var model = Create();
        Assert(!Read<bool>(model, "IsHistory") && !Read<bool>(model, "IsPaused"), "Начальный режим не является активным обзором");
        Assert(Read<string>(model, "CpuAppText") == "—" && History(model).Length == 0, "До первого измерения показаны вымышленные значения");
    }

    // Проверяем данные и уведомления, используемые привязками настоящей страницы
    private static void MeasurementsAndHistory()
    {
        var model = Create();
        var notifications = new List<string>();
        ((INotifyPropertyChanged)model).PropertyChanged += (_, e) => notifications.Add(e.PropertyName!);
        Call(model, "Apply", Sample(0));
        Assert(Read<string>(model, "CpuAppText").Contains('6') && Read<string>(model, "MemoryAppText") != "—", "Измерения не отображены");
        Assert(Read<double>(model, "CpuTotalValue") == 38 && Read<double>(model, "MemoryTotalValue") == 65, "Потеряны значения шкал");
        Assert(History(model).Length == 1 && notifications.Contains("History") && notifications.Contains("CpuAppText"), "Нет обновления привязок и истории");
    }

    // Во время паузы ни карточки, ни графики не меняются
    private static void PausePreservesReadings()
    {
        var model = Create();
        Call(model, "Apply", Sample(0));
        Call(model, "TogglePause");
        Call(model, "Apply", Sample(1, cpu: 72));
        Assert(Read<bool>(model, "IsPaused") && Read<double>(model, "CpuAppValue") == 6 && History(model).Length == 1, "Пауза пропускает обновления");
        Call(model, "TogglePause");
        Call(model, "Apply", Sample(2, cpu: 11));
        Assert(!Read<bool>(model, "IsPaused") && Read<double>(model, "CpuAppValue") == 11, "Продолжение не принимает измерения");
    }

    // Окно истории ограничено минутой, разрыв измерений сохраняется во временных координатах
    private static void MinuteWindowAndGaps()
    {
        var model = Create();
        for (var second = 0; second <= 180; second++) Call(model, "Apply", Sample(second));
        Assert(History(model).Length == 61, "История не ограничена последней минутой");
        Call(model, "Apply", Sample(250));
        Assert(History(model).Length == 1, "Старые точки остались после длительного перерыва");
        Call(model, "Apply", Sample(249));
        Assert(History(model).Length == 1, "Устаревшее измерение нарушило порядок времени");
    }

    // Нулевые показания отличаются от отсутствующих и некорректных
    private static void UnavailableIsNotZero()
    {
        var model = Create();
        Call(model, "Apply", Sample(0, cpu: 0));
        Assert(Read<string>(model, "CpuAppText") != "—", "Настоящий ноль скрыт");
        Call(model, "Apply", Sample(1) with { CpuAppPercent = double.NaN, CpuTotalPercent = double.PositiveInfinity, MemoryTotalGb = 0 });
        Assert(Read<string>(model, "CpuAppText") == "—" && Read<string>(model, "CpuTotalText") == "—", "Ошибка датчика выглядит как нулевая нагрузка");
        Assert(Read<string>(model, "MemoryTotalText") == "—" && !Read<bool>(model, "HasMemoryTotal"), "Отсутствующая системная память выглядит достоверной");
        Assert(History(model).Last().GetType().GetProperty("CpuApp")!.GetValue(History(model).Last()) is null, "График подменяет пропуск нулём");
    }

    // Переключение вкладок не сбрасывает измерения и историю
    private static void ViewChangePreservesData()
    {
        var model = Create();
        Call(model, "Apply", Sample(0));
        Call(model, "SelectView", true);
        Assert(Read<bool>(model, "IsHistory") && History(model).Length == 1, "Динамика сбросила историю");
        Call(model, "SelectView", false);
        Assert(!Read<bool>(model, "IsHistory") && Read<double>(model, "CpuAppValue") == 6, "Возврат в обзор сбросил показатели");
    }

    // Проверяем оба вида и привязки к настоящим измерениям
    private static void HardwareLayout()
    {
        var root = Load("Hardware.xaml");
        Assert(!root.Descendants().Any(x => x.Name.LocalName == "RadialGauge"), "Остались стрелочные индикаторы");
        foreach (var id in new[] { "HardwareViewSelector", "HardwareOverview", "HardwareHistory", "HardwarePause", "CpuHistory", "MemoryHistory" })
            Assert(root.Descendants().Any(x => (string?)x.Attribute("AutomationProperties.AutomationId") == id), $"Нет элемента {id}");
        Assert(root.Descendants().Any(x => x.Name.LocalName == "AdaptiveTrigger"), "Нет адаптивной раскладки");
        Assert(root.ToString().Contains("ViewModel.Dashboard"), "Показатели не связаны с моделью");
    }

    // Оболочка запускается свёрнутой, а кнопка глаза использует общее состояние
    private static void ShellLayout()
    {
        var root = Load("Shell.xaml");
        var navigation = root.Descendants().Single(x => x.Name.LocalName == "NavigationView");
        Assert((string?)navigation.Attribute("IsPaneOpen") == "False", "Меню открыто по умолчанию");
        Assert(!navigation.Descendants().Any(x => x.Name.LocalName == "ImageIconSource"),
            "Иконки меню используют источник, не отображающийся в свёрнутой панели WinUI");
        var toggle = root.Descendants().Single(x => (string?)x.Attribute("AutomationProperties.AutomationId") == "ToggleSwitchShowSensitiveData");
        Assert(toggle.Name.LocalName == "ToggleButton" && toggle.Attribute("IsChecked")?.Value.Contains("ViewModel.IsDisplaySensitiveData") == true,
            "Глаз не связан с общим состоянием данных");
        Assert(toggle.Descendants().Any(x => x.Name.LocalName == "TgOutlineIcon"), "Нет новой контурной иконки глаза");
    }

    // Подписи нового оборудования существуют на обоих языках
    private static void Localization()
    {
        foreach (var language in new[] { "ru-RU", "en-US" })
        {
            var root = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Strings", language, "Resources.resw"));
            foreach (var key in new[] { "HardwareOverview.Text", "HardwareHistory.Text", "HardwarePause", "HardwareResume", "HardwareUnavailable",
                "HardwareCpu.Text", "HardwareMemory.Text", "HardwareSystem.Text", "HardwareNote.Text", "ToolbarSensitiveDataShow", "ToolbarSensitiveDataHide" })
                Assert(root.Descendants("data").Any(x => (string?)x.Attribute("name") == key && !string.IsNullOrWhiteSpace(x.Element("value")?.Value)),
                    $"Нет строки {language}/{key}");
        }
    }

    // Читаем исходную разметку из ресурсов
    private static XDocument Load(string name) => XDocument.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream(name)!);

    // Фиксируем нарушение наблюдаемого результата
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
