using Cornerstone.Database.Models;

namespace Cornerstone.Database.Services;
public interface IScriptService
{
    void CreateScripts(ConnectionStringModel connectionString, DirectoryInfo directory, IProgress<ScriptProgress> progress, string objectFilter = "");
    void ExecuteScripts(ConnectionStringModel connectionString, IEnumerable<FileInfo> fileList, bool continueOnError, IProgress<ScriptProgress> progress);
    string MergeScripts(IEnumerable<string> scripts);
    void MergeScripts(IEnumerable<string> scripts, string toFile);
}
