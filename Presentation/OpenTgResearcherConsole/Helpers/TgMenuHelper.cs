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

    public bool AskQuestionTrueFalseReturnPositive(string title, bool isTrueFirst = false)
    {
        var prompt = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"  {title}?")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(isTrueFirst
                ? new() { TgLocale.MenuIsTrue, TgLocale.MenuIsFalse }
                : new List<string> { TgLocale.MenuIsFalse, TgLocale.MenuIsTrue }));
        return prompt.Equals(TgLocale.MenuIsTrue);
    }

    public bool AskQuestionTrueFalseReturnNegative(string question, bool isTrueFirst = false) =>
        !AskQuestionTrueFalseReturnPositive(question, isTrueFirst);

    public bool AskQuestionYesNoReturnPositive(string title, bool isYesFirst = false)
    {
        var prompt = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"  {title}?")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(isYesFirst
                ? new() { TgLocale.MenuYes, TgLocale.MenuNo }
                : new List<string> { TgLocale.MenuNo, TgLocale.MenuYes }));
        return prompt.Equals(TgLocale.MenuYes);
    }

    public bool AskQuestionYesNoReturnNegative(string question, bool isYesFirst = false) =>
        !AskQuestionYesNoReturnPositive(question, isYesFirst);

    public async Task<TgEfContactEntity> GetContactFromEnumerableAsync(string title, IEnumerable<TgEfContactEntity> items)
    {
        items = items.OrderBy(x => x.Id);
        List<string> list = [TgLocale.MenuMainReturn];
        list.AddRange(items.Select(item => TgLog.GetMarkupString(item.ToConsoleString())));
        var sourceString = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"  {title}")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(list));
        if (!Equals(sourceString, TgLocale.MenuMainReturn))
        {
            var parts = sourceString.Split('|');
            if (parts.Length > 3)
            {
                var sourceId = parts[2].TrimEnd(' ');
                if (long.TryParse(sourceId, out var id))
                    return (await BusinessLogicManager.StorageManager.ContactRepository.GetAsync(new() { Id = id })).Item;
            }
        }
        return (await BusinessLogicManager.StorageManager.ContactRepository.GetNewAsync()).Item;
    }

    public async Task<TgEfFilterEntity> GetFilterFromEnumerableAsync(string title, IEnumerable<TgEfFilterEntity> items)
    {
        items = items.OrderBy(x => x.Name);
        List<string> list = [TgLocale.MenuMainReturn];
        list.AddRange(items.Select(item => TgLog.GetMarkupString(item.ToConsoleString())));
        var sourceString = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"  {title}")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(list));
        if (!Equals(sourceString, TgLocale.MenuMainReturn))
        {
            var parts = sourceString.Split('|');
            if (parts.Length > 3)
            {
                var name = parts[0].TrimEnd(' ');
                return (await BusinessLogicManager.StorageManager.FilterRepository.GetAsync(new() { Name = name })).Item;
            }
        }
        return (await BusinessLogicManager.StorageManager.FilterRepository.GetNewAsync()).Item;
    }

    public async Task<TgEfSourceEntity> GetChatFromEnumerableAsync(string title, IEnumerable<TgEfSourceEntity> items)
    {
        items = items.OrderBy(x => x.UserName).ThenBy(x => x.Title);
        List<string> list = [TgLocale.MenuMainReturn, TgEfSourceEntity.ToHeaderString()];
        list.AddRange(items.Select(item => TgLog.GetMarkupString(item.ToConsoleString())));
        var sourceString = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"  {title}")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(list));
        if (!Equals(sourceString, TgLocale.MenuMainReturn))
        {
            var parts = sourceString.Split('|');
            var sourceId = parts[0].TrimEnd(' ');
            if (long.TryParse(sourceId, out var id))
                return await BusinessLogicManager.StorageManager.SourceRepository.GetItemAsync(new() { Id = id });
        }
        return new();
    }

    public async Task<TgEfStoryEntity> GetStoryFromEnumerableAsync(string title, IEnumerable<TgEfStoryEntity> stories)
    {
        stories = stories.OrderBy(x => x.Id);
        List<string> list = [TgLocale.MenuMainReturn];
        list.AddRange(stories.Select(story => TgLog.GetMarkupString(story.ToConsoleString())));
        var storyString = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"  {title}")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(list));
        if (!Equals(storyString, TgLocale.MenuMainReturn))
        {
            var parts = storyString.Split('|');
            if (parts.Length > 3)
            {
                var sourceId = parts[2].TrimEnd(' ');
                if (long.TryParse(sourceId, out var id))
                    return (await BusinessLogicManager.StorageManager.StoryRepository.GetAsync(new() { Id = id })).Item;
            }
        }
        return (await BusinessLogicManager.StorageManager.StoryRepository.GetNewAsync()).Item;
    }

    public void GetVersionFromEnumerable(string title, IEnumerable<TgEfVersionEntity> versions)
    {
        List<TgEfVersionEntity> versionsList = [.. versions.OrderBy(x => x.Version)];
        List<string> list = [TgLocale.MenuMainReturn];
        list.AddRange(versionsList.Select(version => TgLog.GetMarkupString(version.ToConsoleString())));
        AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"  {title}")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(list));
    }

    #endregion
}