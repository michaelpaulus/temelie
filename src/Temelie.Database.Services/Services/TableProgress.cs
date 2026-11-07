namespace Temelie.Database.Services;

public class TableProgress
{
    public int ProgressPercentage { get; set; }
    public required Models.TableModel Table { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
}
