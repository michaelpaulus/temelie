using System.Windows;
using Cornerstone.Database.Providers;
using Cornerstone.Database.UI;
using Microsoft.Extensions.DependencyInjection;

namespace Cornerstone.Database.Views;

/// <summary>
/// Interaction logic for DatabaseConnectionDialog.xaml
/// </summary>
public partial class DatabaseConnectionDialog : Window
{
    public DatabaseConnectionDialog()
    {
        InitializeComponent();
        var providers = ((IServiceProviderApplication)Application.Current).ServiceProvider.GetServices<IDatabaseProvider>();
        this.DataContext = new ViewModels.DatabaseConnectionViewModel(providers);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
