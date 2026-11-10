namespace OpenTgResearcherDesktop.ViewModels;

public partial class TgSettingsViewModel
{
    private TgAccountProfiles _profileCatalog = null!;
    private TgAccountProfiles.Values? _profileEditSnapshot;
    private Guid? _createdProfileId;
    public ObservableCollection<TgAccountProfile> Profiles => _profileCatalog.Profiles;
    public bool CanSelectProfile => !IsProfileBusy && !IsProfileEditing;
    public IRelayCommand ProfileCreateCommand { get; private set; } = null!;
    public IRelayCommand ProfileEditCommand { get; private set; } = null!;
    public IRelayCommand ProfileCancelEditCommand { get; private set; } = null!;
    public IAsyncRelayCommand ProfileDeleteCommand { get; private set; } = null!;
    public IAsyncRelayCommand ProfileSaveCommand { get; private set; } = null!;
    public IAsyncRelayCommand ProfileApplyCommand { get; private set; } = null!;
    [ObservableProperty]
    public partial TgAccountProfile SelectedProfile { get; set; } = null!;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSelectProfile))]
    public partial bool IsProfileEditing { get; set; }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSelectProfile))]
    public partial bool IsProfileBusy { get; private set; }
    [ObservableProperty]
    public partial string ProfileMessage { get; private set; } = string.Empty;

    // Создаём каталог отдельно от базы, чтобы смена аккаунта не теряла остальные профили
    private void InitializeProfiles()
    {
        _profileCatalog = new(SettingsService, "ProfileDefaultName".GetLocalizedResource());
        SelectedProfile = Profiles.First(x => x.Id == _profileCatalog.ActiveProfileId);
        ProfileCreateCommand = new RelayCommand(CreateProfile, () => CanSelectProfile);
        ProfileEditCommand = new RelayCommand(BeginProfileEdit, () => CanSelectProfile);
        ProfileCancelEditCommand = new RelayCommand(CancelProfileEdit, () => !IsProfileBusy && IsProfileEditing);
        ProfileDeleteCommand = new AsyncRelayCommand(DeleteProfileAsync, () => CanSelectProfile && Profiles.Count > 1);
        ProfileSaveCommand = new AsyncRelayCommand(SettingsSaveAsync, () => !IsProfileBusy && IsProfileEditing);
        ProfileApplyCommand = new AsyncRelayCommand(() => SaveSettingsWithProfileAsync(applySelectedProfile: true), () => !IsProfileBusy);
    }

    // Выбор записи только показывает её поля, применяем аккаунт отдельной командой
    partial void OnSelectedProfileChanged(TgAccountProfile value)
    {
        EndProfileEdit();
        ProfileMessage = string.Empty;
    }

    // Синхронизируем кнопки с явным режимом карточки
    partial void OnIsProfileEditingChanged(bool value) => RefreshProfileCommands();

    // Блокируем повторное применение, пока завершаются операции прежнего аккаунта
    partial void OnIsProfileBusyChanged(bool value)
    {
        RefreshProfileCommands();
        SettingsLoadCommand.NotifyCanExecuteChanged();
        SettingsDefaultCommand.NotifyCanExecuteChanged();
        SettingsSaveCommand.NotifyCanExecuteChanged();
    }

    // Обновляем доступность действий после переключения режима и изменения списка
    private void RefreshProfileCommands()
    {
        ProfileCreateCommand.NotifyCanExecuteChanged();
        ProfileEditCommand.NotifyCanExecuteChanged();
        ProfileCancelEditCommand.NotifyCanExecuteChanged();
        ProfileDeleteCommand.NotifyCanExecuteChanged();
        ProfileSaveCommand.NotifyCanExecuteChanged();
        ProfileApplyCommand.NotifyCanExecuteChanged();
    }

    // Сохраняем значения выбранной записи для локальной отмены редактирования
    private void BeginProfileEdit()
    {
        if (!CanSelectProfile || SelectedProfile is null) return;
        _profileEditSnapshot = new(SelectedProfile.Id, SelectedProfile.Name, SelectedProfile.StoragePath, SelectedProfile.SessionPath);
        ProfileMessage = string.Empty;
        IsProfileEditing = true;
    }

    // Возвращаем исходные поля или убираем несохранённый новый профиль без обращения к его файлам
    private void CancelProfileEdit()
    {
        if (IsProfileBusy || !IsProfileEditing) return;
        if (_profileEditSnapshot is { } original)
        {
            SelectedProfile.Name = original.Name;
            SelectedProfile.StoragePath = original.StoragePath;
            SelectedProfile.SessionPath = original.SessionPath;
        }
        else if (_createdProfileId == SelectedProfile.Id)
        {
            _profileCatalog.Delete(SelectedProfile.Id);
            SelectedProfile = Profiles.FirstOrDefault(x => x.Id == _profileCatalog.ActiveProfileId) ?? Profiles[0];
        }
        EndProfileEdit();
        ProfileMessage = string.Empty;
    }

    // Завершаем режим редактирования после сохранения, отмены или смены записи
    private void EndProfileEdit()
    {
        _profileEditSnapshot = null;
        _createdProfileId = null;
        IsProfileEditing = false;
    }

    // Отменяем несохранённые изменения при загрузке настроек
    private void LoadProfiles()
    {
        _profileCatalog.Reload("ProfileDefaultName".GetLocalizedResource());
        SelectedProfile = Profiles.First(x => x.Id == _profileCatalog.ActiveProfileId);
        EndProfileEdit();
        ProfileDeleteCommand.NotifyCanExecuteChanged();
    }

    // Предлагаем отдельные файлы данных для нового аккаунта без копирования прежней сессии
    private void CreateProfile()
    {
        if (!CanSelectProfile) return;
        try
        {
            var number = 1;
            string name;
            do { name = string.Format(CultureInfo.CurrentCulture, "ProfileNewName".GetLocalizedResource(), number++); }
            while (Profiles.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)));
            var directory = Path.Combine(Path.GetDirectoryName(SettingsService.SettingFile)!, "Profiles", Guid.NewGuid().ToString("N"));
            SelectedProfile = _profileCatalog.Create(name, Path.Combine(directory, "TgStorage.db"), Path.Combine(directory, "OpenTgResearcher.session"));
            _createdProfileId = SelectedProfile.Id;
            IsProfileEditing = true;
            ProfileDeleteCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex) { SetProfileError(ex); }
    }

    // Удаляем только запись после подтверждения, файлы хранилища и сессии остаются на диске
    private async Task DeleteProfileAsync(CancellationToken ct = default)
    {
        if (XamlRootVm is null || !CanSelectProfile || Profiles.Count <= 1) return;
        try
        {
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRootVm,
                Title = "ProfileDeleteTitle".GetLocalizedResource(),
                Content = string.Format(CultureInfo.CurrentCulture, "ProfileDeleteHint".GetLocalizedResource(), SelectedProfile.Name),
                PrimaryButtonText = TgResourceExtensions.GetYesButton(),
                CloseButtonText = TgResourceExtensions.GetCancelButton(),
                DefaultButton = ContentDialogButton.Close,
            };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
            ct.ThrowIfCancellationRequested();
            _profileCatalog.Delete(SelectedProfile.Id);
            SelectedProfile = Profiles.FirstOrDefault(x => x.Id == _profileCatalog.ActiveProfileId) ?? Profiles[0];
            ProfileMessage = "ProfileDeletedDraft".GetLocalizedResource();
            ProfileDeleteCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex) { SetProfileError(ex); }
    }

    // Сверяем выбранные файлы с применённым аккаунтом, а не с другими черновиками
    private bool ProfileNeedsRestart() => SelectedProfile.Id != _profileCatalog.ActiveProfileId
        || !string.Equals(Path.GetFullPath(SelectedProfile.StoragePath), Path.GetFullPath(SettingsService.AppStorage), StringComparison.OrdinalIgnoreCase)
        || !string.Equals(Path.GetFullPath(SelectedProfile.SessionPath), Path.GetFullPath(SettingsService.AppSession), StringComparison.OrdinalIgnoreCase);

    // Сохранение другого профиля не меняет аккаунт, применение и смена открытых файлов освобождают сессию
    private async Task SaveSettingsWithProfileAsync(bool applySelectedProfile = false, CancellationToken ct = default)
    {
        if (XamlRootVm is null || IsProfileBusy) return;
        IsProfileBusy = true;
        try
        {
            _profileCatalog.Validate(SelectedProfile.Id);
            var activeId = applySelectedProfile ? SelectedProfile.Id : _profileCatalog.ActiveProfileId;
            if (!Profiles.Any(x => x.Id == activeId))
                throw new InvalidOperationException("Текущий профиль удалён из списка. Нажмите «Применить профиль» для смены аккаунта "
                    + "или «Загрузить настройки» для отмены удаления");
            var needsRestart = (applySelectedProfile || SelectedProfile.Id == activeId) && ProfileNeedsRestart();
            if (needsRestart && Updates.IsUpdating) throw new InvalidOperationException("Дождитесь завершения обновления программы перед сменой профиля");
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRootVm,
                Title = TgResourceExtensions.AskSettingsSave(),
                Content = (needsRestart ? "ProfileRestartHint" : "ProfileSaveHint").GetLocalizedResource(),
                PrimaryButtonText = TgResourceExtensions.GetYesButton(),
                CloseButtonText = TgResourceExtensions.GetCancelButton(),
                DefaultButton = ContentDialogButton.Close,
            };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
            ct.ThrowIfCancellationRequested();
            SaveProgramPreferences();
            SettingsService.Save();
            if (needsRestart)
            {
                var previous = SettingsService.ReadSetting<TgAccountProfiles.State>(TgAccountProfiles.SettingsKey)!;
                await App.RestartForProfileAsync(SaveProfileFiles, () => SettingsService.SaveProfileState(previous));
            }
            else
            {
                _profileCatalog.Save(activeId);
                SettingsService.Load();
                EndProfileEdit();
                ProfileMessage = "ProfileSaved".GetLocalizedResource();
            }
        }
        catch (Exception ex) { SetProfileError(ex); }
        finally { IsProfileBusy = false; }
    }

    // Создаём каталоги только при применении, сами файлы инициализирует следующий запуск
    private void SaveProfileFiles()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(SelectedProfile.StoragePath))!);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(SelectedProfile.SessionPath))!);
        _profileCatalog.Save(SelectedProfile.Id);
    }

    // Показываем причину ошибки рядом с профилем и сохраняем её в журнале
    private void SetProfileError(Exception ex)
    {
        ProfileMessage = ex.Message;
        LogError(ex);
    }
}
