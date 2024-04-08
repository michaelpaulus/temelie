namespace Cornerstone.Database.Processes;

public class TableProgress
{
    public int ProgressPercentage { get; set; }
    public Models.TableModel Table { get; set; }
    public string ErrorMessage { get; set; }
    public Exception Exception { get; set; }
}
