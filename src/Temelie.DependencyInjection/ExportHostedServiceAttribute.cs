namespace Temelie.DependencyInjection;
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class ExportHostedServiceAttribute : Attribute
{
    public ExportHostedServiceAttribute()
    {
    }
}
