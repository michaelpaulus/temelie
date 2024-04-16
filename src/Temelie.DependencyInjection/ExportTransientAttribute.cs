namespace Temelie.DependencyInjection;
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class ExportTransientAttribute : ExportAttribute
{
    public ExportTransientAttribute(Type forType) : base(forType)
    {
    }
}
