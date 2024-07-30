using Microsoft.CodeAnalysis.Text;

namespace Temelie.Entities.SourceGenerator;

internal class File(string filePath, SourceText content) : IEquatable<File>
{
    public string FilePath { get; } = filePath;
    public SourceText Content { get; } = content;

    public bool Equals(File other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }
        return ReferenceEquals(this, other) ||
            FilePath == other.FilePath &&
            Content.Length == other.Content.Length;
    }

    public override bool Equals(object obj)
    {
        return obj is File file && Equals(file);
    }

    public override int GetHashCode()
    {
        Repository.SourceGenerator.HashCode hash = new();
        hash.Add(FilePath);
        hash.Add(Content);
        return hash.GetHashCode();
    }

}
