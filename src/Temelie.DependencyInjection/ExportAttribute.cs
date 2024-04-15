namespace Temelie.DependencyInjection;
public abstract class ExportAttribute(Type forType) : Attribute
{
    public Type ForType { get; } = forType;
    public int Priority { get; set; } = int.MaxValue;
}
