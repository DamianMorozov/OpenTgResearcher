namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgClientConnectionViewModel
{
    private bool _isConnectionActive;
    public TgClientSignInFlow SignIn { get; } = new();
    public IRelayCommand OpenSignInCommand { get; }
    public IRelayCommand CloseSignInCommand { get; }
    public IRelayCommand SignInBackCommand { get; }
    public IAsyncRelayCommand SignInNextCommand { get; }
    public IRelayCommand OpenChatListCommand { get; }
    public IRelayCommand OpenProfileSettingsCommand { get; }
    [ObservableProperty]
    public partial string LoginEmail { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string ApiIdInput { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string ApiHashInput { get; set; } = string.Empty;
    public string ProfileName => TgAccountProfiles.ReadActive(SettingsService)?.Name ?? "ProfileDefaultName".GetLocalizedResource();
    public string SessionFileName => IsDisplaySensitiveData ? Path.GetFileName(SettingsService.AppSession) : SensitiveData;
    public string AccountDisplay => !IsDisplaySensitiveData ? SensitiveData : App.BusinessLogicManager.ConnectClient.Me is { } me
        ? $"{me.first_name} {me.last_name}{(string.IsNullOrWhiteSpace(me.MainUsername) ? string.Empty : $" · @{me.MainUsername}")}".Trim()
        : "ConnectionAccountPending".GetLocalizedResource();
    public string ConnectionStatus => (IsOnlineReady ? "ConnectionConnected" : SignIn.HasRequest ? "ConnectionWaiting" : "ConnectionDisconnected").GetLocalizedResource();
    public string WizardStepCaption => string.Format("ConnectionStepCaption".GetLocalizedResource(), SignIn.Step + 1);
    public string WizardNextCaption => (SignIn.Step switch
    {
        0 => "ConnectionNext", 1 => "ConnectionConnect", 2 => "ConnectionConfirm", _ => "HomeOpenChats",
    }).GetLocalizedResource();
    public string AuthorizationHint => (SignIn.Request switch
    {
        "password" => "ConnectionPasswordHelp", "name" => "ConnectionNameHelp", "email" => "ConnectionEmailHelp",
        "email_verification_code" => "ConnectionEmailCodeHelp", _ => "ConnectionCodeHelp",
    }).GetLocalizedResource();
    public bool HasConnectionMessage => !string.IsNullOrWhiteSpace(ConnectionMsg);
    public string DisplayConnectionMessage => IsDisplaySensitiveData ? ConnectionMsg : SensitiveData;

    // Выбор формы не создаёт клиент Telegram и не изменяет текущие реквизиты
    private void InitializeSignIn()
    {
        var key = $"{TgAccountProfiles.ReadActive(SettingsService)?.Id}|{SettingsService.AppStorage}|{SettingsService.AppSession}";
        SignIn.Initialize(key, hasSession: IsExistsAppSession && AppDto?.Uid != Guid.Empty, isOnline: IsOnlineReady);
        if (SignIn.HasRequest && App.BusinessLogicManager.ConnectClient.Client is null) SignIn.Reset();
        RefreshConnectionPresentation();
    }

    // Общие черновики обновляют все экземпляры полей центра и мастера
    partial void OnApiIdInputChanged(string value) { if (AppDto is not null) AppDto.ApiIdString = value; }

    // Пустой хеш отображается подсказкой, а не маской из нулей
    partial void OnApiHashInputChanged(string value) { if (AppDto is not null) AppDto.ApiHashString = value; }

    // Переносим загруженные реквизиты в поля после смены записи приложения
    partial void OnAppDtoChanged(TgEfAppDto? value)
    {
        ApiIdInput = value?.ApiId > 0 ? value.ApiIdString : string.Empty;
        ApiHashInput = value is not null && value.ApiHash != Guid.Empty ? value.ApiHashString : string.Empty;
    }

    // Передаём лишь то поле, которое действительно запросил Telegram
    private string? GetSignInInput() => SignIn.Request switch
    {
        "verification_code" or "email_verification_code" => VerificationCode.Trim(),
        "password" => Password,
        "name" => $"{AppDto?.FirstName}\n{AppDto?.LastName}",
        "email" => LoginEmail.Trim(),
        "" => null,
        _ => throw new InvalidOperationException($"Telegram запросил неподдерживаемое поле: {SignIn.Request}"),
    };

    // Первая кнопка проверяет API локально, последующие продолжают реальный вход
    private async Task SignInNextAsync()
    {
        if (SignIn.IsBusy) return;
        if (SignIn.IsDoneStep) { OpenChatListCommand.Execute(null); return; }
        if (SignIn.IsApiStep)
        {
            if (!SignIn.AdvanceToPhone(AppDto?.ApiId ?? 0, AppDto?.ApiHash ?? Guid.Empty))
                ConnectionMsg = TgResourceExtensions.GetClientApiSettingsHelp();
            else { Exception.Default(); ConnectionMsg = string.Empty; }
            RefreshConnectionPresentation();
            return;
        }
        await ClientConnectCoreAsync(isQuestion: false);
    }

    // Обновляем вычисляемые подписи после перехода между шагами
    private void OnSignInChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => RefreshConnectionPresentation();

    // Общий переключатель конфиденциальности скрывает также аккаунт, сессию и сообщения формы
    private void OnConnectionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(IsDisplaySensitiveData) or nameof(IsOnlineReady) or nameof(ConnectionMsg)) RefreshConnectionPresentation();
    }

    // Строки пересчитываются при смене языка без повторной авторизации
    private void OnConnectionLanguageChanged(object? sender, LanguageChangedEventArgs e) => TgDesktopUtils.InvokeOnUIThread(RefreshConnectionPresentation);

    /// <summary> Подписывает открытую страницу на смену языка </summary>
    public void Activate()
    {
        if (_isConnectionActive) return;
        _isConnectionActive = true;
        Localizer.Get().LanguageChanged += OnConnectionLanguageChanged;
    }

    /// <summary> Убирает подписку страницы без отключения Telegram </summary>
    public void Deactivate()
    {
        if (!_isConnectionActive) return;
        _isConnectionActive = false;
        Localizer.Get().LanguageChanged -= OnConnectionLanguageChanged;
    }

    /// <summary> Освобождает подписку страницы при завершении приложения </summary>
    public override void ReleaseManagedResources() { Deactivate(); base.ReleaseManagedResources(); }

    // Обновляем только представление, не выполняя сетевых запросов
    private void RefreshConnectionPresentation()
    {
        foreach (var property in new[] { nameof(ProfileName), nameof(SessionFileName), nameof(AccountDisplay), nameof(ConnectionStatus),
            nameof(WizardStepCaption), nameof(WizardNextCaption), nameof(AuthorizationHint), nameof(HasConnectionMessage), nameof(DisplayConnectionMessage) })
            OnPropertyChanged(property);
    }
}
