using System.Diagnostics;

namespace Cornerstone.Example.Configuration;

public sealed class DirectoryConfig
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

    public static string Branch => _branch.Value;

    private static readonly Lazy<string> _branch = new(() =>
    {
#if DEBUG
        try
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "branch",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Environment.CurrentDirectory
                }
            };

            proc.Start();

            if (proc.StandardOutput is not null)
            {
                while (!proc.StandardOutput.EndOfStream)
                {
                    string? line = proc.StandardOutput.ReadLine();
                    if (line is not null && line.StartsWith("*"))
                    {
                        return line.Substring(1).Trim();
                    }
                }
            }


        }
        catch
        {
        }
#endif

        return "";
    });

}
