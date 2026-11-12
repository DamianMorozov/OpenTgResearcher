namespace OpenTgResearcherDesktop.UpdateSelfTests;

// Проверяем первый вход нового профиля, повторный вызов мастера и переходы по настоящим запросам авторизации
internal static partial class Program
{
    private static async Task ClientSignInFlowAsync()
    {
        var flow = new TgClientSignInFlow();
        flow.Initialize("profile-a", hasSession: false, isOnline: false);
        Assert(flow.IsApiStep && !flow.IsCenter, "Новый профиль не открывает первый шаг");
        Assert(!flow.AdvanceToPhone(0, Guid.Empty) && flow.IsApiStep, "Пустые API-параметры пропустили первый шаг");
        Assert(flow.AdvanceToPhone(123, Guid.NewGuid()) && flow.IsPhoneStep, "Проверенные API-параметры не открывают телефон");
        flow.Back();
        Assert(flow.IsApiStep, "Возврат не открывает API-параметры");
        flow.AdvanceToPhone(123, Guid.NewGuid());
        var calls = 0;
        await flow.SubmitAsync(() => { calls++; return Task.FromResult<string?>("verification_code"); });
        Assert(calls == 1 && flow.IsAuthStep && flow.IsCodeRequested && !flow.IsPasswordRequested,
            "Запрос Telegram не открыл только поле кода");
        flow.Close();
        Assert(flow.IsCenter && flow.HasRequest, "Закрытие мастера потеряло ожидаемый код");
        flow.Initialize("profile-a", hasSession: false, isOnline: false);
        Assert(flow.IsCenter, "Навигация повторно навязала мастер того же профиля");
        flow.Open(isOnline: false);
        Assert(flow.IsAuthStep, "Ручной вызов не вернул ожидаемый запрос");
        try { await flow.SubmitAsync(() => throw new InvalidOperationException("PHONE_CODE_INVALID")); }
        catch (InvalidOperationException) { }
        Assert(flow.IsCodeRequested && flow.IsAuthStep && flow.IsIdle, "Неверный код потерял шаг или оставил форму занятой");
        await flow.SubmitAsync(() => Task.FromResult<string?>("password"));
        Assert(flow.IsPasswordRequested && !flow.IsCodeRequested && !flow.CanGoBack, "Запрос 2FA отображает неверные поля");
        var release = new TaskCompletionSource<string?>();
        var pending = flow.SubmitAsync(() => release.Task);
        await flow.SubmitAsync(() => { calls++; return Task.FromResult<string?>(null); });
        Assert(flow.IsBusy && calls == 1, "Повторное нажатие запустило второй запрос входа");
        release.SetResult(null);
        await pending;
        Assert(flow.IsDoneStep && !flow.HasRequest && flow.IsIdle, "Подтверждённый вход не завершил мастер");
        flow.Close(); flow.Open(isOnline: true);
        Assert(flow.IsDoneStep, "Ручной вызов отключает готовый аккаунт");
        flow.Initialize("profile-b", hasSession: true, isOnline: false);
        Assert(flow.IsCenter && !flow.HasRequest, "Готовая сессия требует нового входа или наследует запрос другого профиля");
        flow.Initialize("profile-c", hasSession: false, isOnline: false);
        Assert(flow.IsApiStep, "Другой новый профиль не открывает мастер");
        foreach (var request in new[] { "name", "email", "email_verification_code" })
        {
            await flow.SubmitAsync(() => Task.FromResult<string?>(request));
            Assert(flow.Request == request && flow.IsAuthStep, $"Потерян дополнительный запрос {request}");
        }
        flow.Reset();
        Assert(!flow.HasRequest && flow.IsApiStep, "Сброс сессии сохранил прошлый запрос");
    }
}
