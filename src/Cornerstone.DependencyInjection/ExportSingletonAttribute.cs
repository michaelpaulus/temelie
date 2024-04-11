namespace Cornerstone.DependencyInjection;
public class ExportSingletonAttribute : ExportAttribute
{
    public ExportSingletonAttribute(Type forType) : base(forType)
    {
    }
}
