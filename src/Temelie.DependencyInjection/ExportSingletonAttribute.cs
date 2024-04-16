namespace Temelie.DependencyInjection;
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class ExportSingletonAttribute : ExportAttribute
{
    public ExportSingletonAttribute(Type forType) : base(forType)
    {
    }

}
