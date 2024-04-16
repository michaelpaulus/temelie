namespace Temelie.DependencyInjection;
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class ExportProviderAttribute : ExportAttribute
{
    public ExportProviderAttribute(Type forType) : base(forType)
    {
    }
}
