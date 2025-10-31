using Temelie.Database.Models;
using Temelie.Database.Providers;

namespace Temelie.Database.Services;
public interface IScriptService
{
    void CreateScripts(ConnectionStringModel connectionString, DirectoryInfo directory, Action<ScriptProgress> progress, string objectFilter = "", IDatabaseProvider createScriptsProvider = null);
    void ExecuteScripts(ConnectionStringModel connectionString, DirectoryInfo directory, Action<ScriptProgress> progress, bool continueOnError = true, bool skipMigrations = false);
    string MergeScripts(IEnumerable<string> scripts);
    void MergeScripts(IEnumerable<string> scripts, string toFile);
}
