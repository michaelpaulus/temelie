using System.Windows;
using Temelie.Database.Providers;
using Temelie.Database.UI;
using Microsoft.Extensions.DependencyInjection;

namespace Temelie.Database.Views;

/// <summary>
/// Interaction logic for DatabaseConnectionDialog.xaml
/// </summary>
public partial class DatabaseConnectionDialog : Window
{
    public DatabaseConnectionDialog()
    {
        InitializeComponent();
        var providers = ServiceProviderApplication.ServiceProvider.GetServices<IDatabaseProvider>();
        this.DataContext = new ViewModels.DatabaseConnectionViewModel(providers);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
