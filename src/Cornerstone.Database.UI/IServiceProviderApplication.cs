using System;
using System.Windows;

namespace Temelie.Database.UI;

public static class ServiceProviderApplication
{
    public static IServiceProvider ServiceProvider => ((IServiceProviderApplication)Application.Current).ServiceProvider;
}

public interface IServiceProviderApplication
{

    public IServiceProvider ServiceProvider { get; }

}
