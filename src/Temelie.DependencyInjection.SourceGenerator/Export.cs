namespace Temelie.DependencyInjection.SourceGenerator;
public readonly record struct Export(string Type, string ForType, bool IsSingleton, bool IsTransient, bool IsProvider, bool IsStartupConfig, bool IsHosted, int Priority)
{
}
