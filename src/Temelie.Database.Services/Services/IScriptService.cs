using Temelie.Database.Models;

namespace Temelie.Database.Services;
public interface IScriptService
{
    void CreateScripts(ConnectionStringModel connectionString, DirectoryInfo directory, IProgress<ScriptProgress> progress, string objectFilter = "");
    void ExecuteScripts(ConnectionStringModel connectionString, DirectoryInfo directory, IProgress<ScriptProgress> progress, bool continueOnError = true);
    string MergeScripts(IEnumerable<string> scripts);
    void MergeScripts(IEnumerable<string> scripts, string toFile);
}
