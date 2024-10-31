namespace Temelie.Repository;
public interface IModelBuilderContext 
{
    IEnumerable<IModelBuilderProvider> Providers { get; }
}
