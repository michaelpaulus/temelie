namespace Temelie.Database.Testing;

/// <summary>A temporary script folder (like a database project) that is deleted on dispose.</summary>
public sealed class ScriptFolder : IDisposable
{
    public ScriptFolder()
    {
        Directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "temelie-tests", Guid.NewGuid().ToString("N")));
        Directory.Create();
    }

    public DirectoryInfo Directory { get; }

    public string PathOf(string relativePath) => Path.Combine(Directory.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));

    public string Write(string relativePath, string contents)
    {
        var path = PathOf(relativePath);
        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, contents);
        return path;
    }

    public void CreateDirectory(string relativePath) => System.IO.Directory.CreateDirectory(PathOf(relativePath));

    public string Read(string relativePath) => File.ReadAllText(PathOf(relativePath));

    public bool Exists(string relativePath) => File.Exists(PathOf(relativePath)) || System.IO.Directory.Exists(PathOf(relativePath));

    /// <summary>All files, as forward-slash paths relative to the folder.</summary>
    public IReadOnlyList<string> Files() => System.IO.Directory
        .GetFiles(Directory.FullName, "*", SearchOption.AllDirectories)
        .Select(i => Path.GetRelativePath(Directory.FullName, i).Replace(Path.DirectorySeparatorChar, '/'))
        .Order(StringComparer.Ordinal)
        .ToList();

    public void Dispose()
    {
        if (Directory.Exists)
        {
            Directory.Delete(true);
        }
    }
}
