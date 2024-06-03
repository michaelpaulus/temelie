namespace Temelie.DependencyInjection;
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class ExportScopedAttribute : ExportAttribute
{
    public ExportScopedAttribute(Type forType) : base(forType)
    {
    }

}
