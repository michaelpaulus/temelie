namespace Temelie.SourceGenerator.Configuration;

internal sealed class DirectoryConfig
{

    public static readonly Lazy<string> _repoDirectory = new(() =>
    {
        var repoDirectory = "";

        var repoRoot = new DirectoryInfo(Environment.CurrentDirectory);

        while (string.IsNullOrEmpty(repoDirectory) &&
            repoRoot is not null &&
            repoRoot.Exists &&
            repoRoot.Parent != null)
        {
            var repoFile = new FileInfo(Path.Combine(repoRoot.FullName, ".repo-root"));
            if (repoFile.Exists)
            {
                repoDirectory = repoRoot.FullName;
            }
            else
            {
                repoRoot = repoRoot.Parent;
            }
        }

        return repoDirectory;
    });

    public static string RepoDirectory => _repoDirectory.Value;

}
