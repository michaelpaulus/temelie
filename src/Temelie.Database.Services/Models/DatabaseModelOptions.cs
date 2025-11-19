namespace Temelie.Database.Models;
public class DatabaseModelOptions
{
    public string ObjectFilter { get; set; } = "";
    public bool ExcludeDoubleUnderscoreObjects { get; set; } = true;
    public bool ExcludeCdcObjects { get; set; } = true;

}
