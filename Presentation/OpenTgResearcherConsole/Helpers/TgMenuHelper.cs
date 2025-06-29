// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

[DebuggerDisplay("{ToDebugString()}")]
internal sealed partial class TgMenuHelper : TgDisposable, ITgHelper
{
    #region Public and internal fields, properties, constructor

    internal static TgAppSettingsHelper TgAppSettings => TgAppSettingsHelper.Instance;
    internal static TgLocaleHelper TgLocale => TgLocaleHelper.Instance;
    internal static TgLogHelper TgLog => TgLogHelper.Instance;
    internal static Style StyleMain => new(Color.White, null, Decoration.Bold | Decoration.Conceal | Decoration.Italic);
    internal TgEnumMenuMain Value { get; set; }
    internal ITgBusinessLogicManager BusinessLogicManager { get; } = default!;
    private static Mutex? _instanceMutex;

    public TgMenuHelper()
    {
        BusinessLogicManager = TgGlobalTools.Container.Resolve<ITgBusinessLogicManager>();
    }

    #endregion

    #region IDisposable

    /// <summary> Release managed resources </summary>
    public override void ReleaseManagedResources()
    {
        BusinessLogicManager.Dispose();
        _instanceMutex?.Dispose();
        ClientForSendData?.Dispose();
        BotForSendData?.Dispose();
    }

    public override void ReleaseUnmanagedResources()
    {
        //
    }

    #endregion

    #region Public and internal methods

    public string ToDebugString() => TgConstants.UseOverrideMethod;

    internal Markup GetMarkup(string message, Style? style = null) => new(message, style);

    internal async Task SetStorageVersionAsync()
    {
        var version = (await BusinessLogicManager.StorageManager.VersionRepository.GetListAsync(TgEnumTableTopRecords.All, 0))
            .Items.Single(x => x.Version == BusinessLogicManager.StorageManager.VersionRepository.LastVersion);
        TgAppSettings.StorageVersion = $"v{version.Version}";
    }

    public static bool AskQuestionTrueFalseReturnPositive(string title, bool isTrueFirst = false) =>
        AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"  {title}?")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(isTrueFirst
                ? new() { TgLocale.MenuIsTrue, TgLocale.MenuIsFalse }
                : new List<string> { TgLocale.MenuIsFalse, TgLocale.MenuIsTrue }))
            .Equals(TgLocale.MenuIsTrue);

    public static bool AskQuestionTrueFalseReturnNegative(string question, bool isTrueFirst = false) =>
        !AskQuestionTrueFalseReturnPositive(question, isTrueFirst);

    public static bool AskQuestionYesNoReturnPositive(string title, bool isYesFirst = false) =>
        AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"  {title}?")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(isYesFirst
                ? [TgLocale.MenuYes, TgLocale.MenuNo]
                : new List<string> { TgLocale.MenuNo, TgLocale.MenuYes }))
            .Equals(TgLocale.MenuYes);

    public static bool AskQuestionYesNoReturnNegative(string question, bool isYesFirst = false) =>
        !AskQuestionYesNoReturnPositive(question, isYesFirst);

    /// <summary> Get DTO from IEnumerable </summary>
    public async Task<TDto> GetDtoFromEnumerableAsync<TEfEntity, TDto>(string title, IEnumerable<TDto> dtos, 
        ITgEfRepository<TEfEntity, TDto> repository)
        where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
        where TDto : class, ITgDto<TEfEntity, TDto>, new()
    {
        List<TgListWithUidDto> listWithUidDtos = [new TgListWithUidDto(TgLocale.MenuReturn), new TgListWithUidDto(new TDto().ToConsoleHeaderString())];
        listWithUidDtos.AddRange(dtos.Select(dto => new TgListWithUidDto(dto.Uid, TgLog.GetMarkupString(dto.ToConsoleString()))));
        var printString = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"  {title}")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(listWithUidDtos.Select(x => x.PrintString)));
        if (!Equals(printString, TgLocale.MenuReturn) && !Equals(printString, new TDto().ToConsoleHeaderString()))
        {
            var uid = listWithUidDtos.FirstOrDefault(x => x.PrintString.Equals(printString))?.Uid ?? Guid.Empty;
            if (uid != Guid.Empty)    
                return await repository.GetDtoAsync(x => x.Uid == uid);
        }
        return new();
    }

    /// <summary> Check multiple instances of the application </summary>
    internal bool CheckMultipleInstances()
    {
        // Use Mutex to check for other instances
        bool createdNew;
        string mutexName = $"Global\\{TgConstants.OpenTgResearcherConsole}_SingleInstance";

        try
        {
            _instanceMutex = new Mutex(true, mutexName, out createdNew);
            if (!createdNew && WaitHandle.WaitAny([_instanceMutex], 1_000) == WaitHandle.WaitTimeout)
            {
                createdNew = false;
            }
        }
        catch (AbandonedMutexException)
        {
            createdNew = true; // Taking the abandoned mutex
        }
        catch (UnauthorizedAccessException ex)
        {
            TgLog.WriteLine($"{TgLocale.LicenseMutexAccessError}: {ex.Message}");
            return true; // Blocking the launch in case of rights errors
        }
        catch (Exception ex)
        {
            TgLog.WriteLine($"  {TgLocale.LicenseErrorCheckingMutex}: {ex.Message}");
            return true; // Blocking the launch in case of other errors
        }

        if (!createdNew)
        {
            // License verification: if it is free, we do not allow the launch of multiple copies
            if (BusinessLogicManager.LicenseService.CurrentLicense.LicenseType == TgEnumLicenseType.Free)
            {
                TgLog.WriteLine("");
                TgLog.WriteLine($"  {TgLocale.LicenseLimitByMultipleInstances}");
                TgLog.WriteLine("");
                return true; // Forbidding the launch
            }
            // For paid licenses, we allow the launch
        }

        return false; // Allowing the launch
    }

    #endregion
}