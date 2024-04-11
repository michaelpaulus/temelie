namespace Cornerstone.DependencyInjection;
public class ExportTransientAttribute : ExportAttribute
{
    public ExportTransientAttribute(Type forType) : base(forType)
    {
    }
}
