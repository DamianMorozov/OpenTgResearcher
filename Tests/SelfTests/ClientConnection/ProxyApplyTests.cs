namespace OpenTgResearcherDesktop.ConnectionSelfTests;

internal static partial class Program
{
    // Применение из таблицы должно сохранить новый прокси, не меняя API-параметры и не подключаясь к Telegram
    private static async Task ApplyProxyFromNavigationAsync()
    {
        var storage = App.BusinessLogicManager.StorageManager;
        var profile = storage.AppRepository.Current;
        profile.Uid = Guid.NewGuid(); profile.ApiId = 123; profile.ApiHash = Guid.NewGuid(); profile.PhoneNumber = "+10000000000";
        var hash = profile.ApiHash;
        var proxy = new TgEfProxyEntity { Uid = Guid.NewGuid(), Type = TgEnumProxyType.Socks };
        storage.ProxyRepository.Items.Add(proxy);
        var vm = CreateViewModel();
        vm.Exception.Set(new InvalidOperationException("Ошибка предыдущего применения"));
        await vm.OnNavigatedToAsync(new() { Parameter = proxy.Uid });
        Assert(vm.ProxyVm?.Dto.Uid == proxy.Uid && vm.AppDto?.ProxyUid == proxy.Uid
            && storage.AppRepository.SavedProxyUid == proxy.Uid && storage.AppRepository.SaveCalls == 1,
            "Прокси из таблицы не выбран или не сохранён в профиле подключения");
        Assert(vm.AppDto!.ApiId == 123 && vm.AppDto.ApiHash == hash && vm.AppDto.PhoneNumber == "+10000000000"
            && App.BusinessLogicManager.ConnectClient.ConnectCalls == 0 && !vm.Exception.IsExist,
            "Применение изменило API-параметры, запустило подключение или сохранило прежнюю ошибку");
    }

    // Новая форма сохраняет выбор до ввода API-параметров без создания невалидного профиля
    private static async Task ApplyProxyToNewProfileAsync()
    {
        var proxy = new TgEfProxyEntity { Uid = Guid.NewGuid(), Type = TgEnumProxyType.Http };
        App.BusinessLogicManager.StorageManager.ProxyRepository.Items.Add(proxy);
        var vm = CreateViewModel();
        await vm.OnNavigatedToAsync(new() { Parameter = proxy.Uid });
        Assert(vm.ProxyVm?.Dto.Uid == proxy.Uid && vm.AppDto?.ProxyUid == proxy.Uid
            && App.BusinessLogicManager.StorageManager.AppRepository.SaveCalls == 0 && !vm.Exception.IsExist,
            "Выбор для новой формы потерян или создан невалидный профиль");
    }

    // Удалённая запись должна давать видимую ошибку без сохранения случайного прокси
    private static async Task ApplyMissingProxyAsync()
    {
        var vm = CreateViewModel();
        await vm.OnNavigatedToAsync(new() { Parameter = Guid.NewGuid() });
        Assert(vm.Exception.IsExist && vm.Errors.Count == 1 && App.BusinessLogicManager.StorageManager.AppRepository.SaveCalls == 0,
            "Применение отсутствующего прокси молча проигнорировано");
    }

    // Ошибка сохранения не должна выдавать успешное применение
    private static async Task ApplyProxySaveFailureAsync()
    {
        var storage = App.BusinessLogicManager.StorageManager;
        storage.AppRepository.Current.Uid = Guid.NewGuid();
        storage.AppRepository.FailSave = true;
        var proxy = new TgEfProxyEntity { Uid = Guid.NewGuid(), Type = TgEnumProxyType.Socks };
        storage.ProxyRepository.Items.Add(proxy);
        var vm = CreateViewModel();
        vm.ConnectionMsg = "Успех предыдущего применения";
        await vm.OnNavigatedToAsync(new() { Parameter = proxy.Uid });
        Assert(vm.Exception.IsExist && vm.Errors.Count == 1 && storage.AppRepository.SaveCalls == 1
            && storage.AppRepository.SavedProxyUid is null && vm.ConnectionMsg == vm.Exception.Message,
            "Ошибка сохранения применяемого прокси скрыта или остался прежний успешный статус");
    }
}
