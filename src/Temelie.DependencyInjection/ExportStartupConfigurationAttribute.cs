namespace Temelie.DependencyInjection;
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class ExportStartupConfigurationAttribute : Attribute
{
    public ExportStartupConfigurationAttribute()
    {
    }
}
