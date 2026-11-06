namespace OpenTgResearcherDesktop.Models;

/// <summary> Сохранённый результат расчёта активности автора с долей сообщений и относительной длиной полосы </summary>
public sealed record TgChatActivityRow(TgEfUserDto User, int Messages, double Share, double Relative)
{
    public string ShareText => $"{Share:0.#} %";
}
