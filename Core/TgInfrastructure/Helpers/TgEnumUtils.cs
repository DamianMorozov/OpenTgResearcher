namespace TgInfrastructure.Helpers;

/// <summary> Enum utilities </summary>
public static class TgEnumUtils
{
	public static string GetLanguageAsString(TgEnumLanguage language) =>
		language switch
		{
			TgEnumLanguage.Russian => TgConstants.LocaleRuRu,
			TgEnumLanguage.English => TgConstants.LocaleEnUs,
			TgEnumLanguage.Default => TgConstants.LocaleEnUs,
			_ => TgConstants.LocaleEnUs
        };
	
	public static string GetLanguage(string language) =>
		language switch
		{
			nameof(TgEnumLanguage.Russian) => TgConstants.LocaleRuRu,
			nameof(TgEnumLanguage.English) => TgConstants.LocaleEnUs,
			nameof(TgEnumLanguage.Default) => TgConstants.LocaleEnUs,
			_ => TgConstants.LocaleEnUs
        };

	public static TgEnumLanguage GetLanguageAsEnum(string language) =>
		language switch
		{
			nameof(TgEnumLanguage.Russian) => TgEnumLanguage.Russian, 
            "ru-RU" => TgEnumLanguage.Russian,
			nameof(TgEnumLanguage.English) => TgEnumLanguage.English, 
            "en-US" => TgEnumLanguage.English,
			nameof(TgEnumLanguage.Default) => TgEnumLanguage.Default, 
            _ => TgEnumLanguage.Default
		};
}
