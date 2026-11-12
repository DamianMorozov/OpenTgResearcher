namespace OpenTgResearcherDesktop.Models;

/// <summary> Управляет экраном первого входа без хранения кода и пароля между запросами </summary>
public sealed partial class TgClientSignInFlow : ObservableObject
{
    private string _profileKey = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCenter), nameof(IsApiStep), nameof(IsPhoneStep), nameof(IsAuthStep), nameof(IsDoneStep))]
    public partial bool IsWizard { get; set; }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsApiStep), nameof(IsPhoneStep), nameof(IsAuthStep), nameof(IsDoneStep), nameof(CanGoBack))]
    public partial int Step { get; set; }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCodeRequested), nameof(IsPasswordRequested), nameof(IsNameRequested),
        nameof(IsEmailRequested), nameof(HasRequest), nameof(CanEditCredentials))]
    public partial string Request { get; private set; } = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIdle), nameof(CanGoBack), nameof(CanEditCredentials))]
    public partial bool IsBusy { get; private set; }
    public bool IsCenter => !IsWizard;
    public bool IsApiStep => IsWizard && Step == 0;
    public bool IsPhoneStep => IsWizard && Step == 1;
    public bool IsAuthStep => IsWizard && Step == 2;
    public bool IsDoneStep => IsWizard && Step == 3;
    public bool IsIdle => !IsBusy;
    public bool CanEditCredentials => !IsBusy && !HasRequest;
    public bool CanGoBack => IsWizard && Step == 1 && !IsBusy;
    public bool HasRequest => Request.Length > 0;
    public bool IsCodeRequested => Request is "verification_code" or "email_verification_code";
    public bool IsPasswordRequested => Request == "password";
    public bool IsNameRequested => Request == "name";
    public bool IsEmailRequested => Request == "email";

    /// <summary> Автоматически открывает мастер только при первом показе нового профиля </summary>
    public void Initialize(string profileKey, bool hasSession, bool isOnline)
    {
        if (_profileKey == profileKey) return;
        _profileKey = profileKey;
        Request = string.Empty;
        Step = 0;
        IsWizard = !hasSession && !isOnline;
    }

    /// <summary> Открывает мастер без отключения действующего аккаунта и очистки реквизитов </summary>
    public void Open(bool isOnline)
    {
        Step = isOnline ? 3 : HasRequest ? 2 : 0;
        IsWizard = true;
    }

    /// <summary> Возвращает центр подключения, сохраняя текущий запрос Telegram </summary>
    public void Close() => IsWizard = false;

    /// <summary> Возвращает к параметрам приложения до отправки телефона </summary>
    public void Back() { if (CanGoBack) Step = 0; }

    /// <summary> Проверяет API-параметры перед переходом к номеру телефона </summary>
    public bool AdvanceToPhone(int apiId, Guid apiHash)
    {
        if (apiId <= 0 || apiHash == Guid.Empty) return false;
        Step = 1;
        return true;
    }

    /// <summary> Выполняет один запрос входа и показывает только реально запрошенное Telegram поле </summary>
    public async Task SubmitAsync(Func<Task<string?>> login, CancellationToken ct = default)
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            ct.ThrowIfCancellationRequested();
            var request = await login();
            ct.ThrowIfCancellationRequested();
            Request = request ?? string.Empty;
            Step = HasRequest ? 2 : 3;
        }
        finally { IsBusy = false; }
    }

    /// <summary> Сбрасывает только состояние формы после явного удаления сессии </summary>
    public void Reset() { Request = string.Empty; Step = 0; }
}
