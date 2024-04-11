namespace Cornerstone.DependencyInjection.SourceGenerator;
public class Export
{
    public string Type { get; set; }
    public string ForType { get; set; }
    public bool IsSingleton { get; set; }
    public bool IsTransient { get; set; }
    public bool IsProvider { get; set; }
    public int Priority { get; set; }

}
