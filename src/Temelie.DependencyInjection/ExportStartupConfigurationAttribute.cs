namespace Temelie.DependencyInjection;
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class ExportStartupConfigurationAttribute : Attribute
{
    public int Priority { get; set; } = int.MaxValue;
}
