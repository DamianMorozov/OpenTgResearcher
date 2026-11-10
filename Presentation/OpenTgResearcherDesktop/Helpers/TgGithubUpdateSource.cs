namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Источник обновлений с признаком предварительного выпуска из метаданных GitHub </summary>
internal sealed class TgGithubUpdateSource(bool prerelease) : GithubSource(TgConstants.LinkGitHub, string.Empty, prerelease)
{
    /// <summary> Учитывает и метку версии, и флаг предварительного выпуска GitHub </summary>
    public bool IsPreviewRelease(VelopackAsset asset) => asset.Version.IsPrerelease
        || (asset is GitBaseAsset githubAsset ? githubAsset.Release.Prerelease : Prerelease);
}
