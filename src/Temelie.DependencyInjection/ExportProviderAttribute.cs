namespace Temelie.DependencyInjection;
public class ExportProviderAttribute : ExportAttribute
{
    public ExportProviderAttribute(Type forType) : base(forType)
    {
    }
}
