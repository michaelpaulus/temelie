using System.Windows;

namespace Cornerstone.Database.Views;

/// <summary>
/// Interaction logic for DatabaseConnectionDialog.xaml
/// </summary>
public partial class DatabaseConnectionDialog : Window
{
    public DatabaseConnectionDialog()
    {
        InitializeComponent();
        this.DataContext = new ViewModels.DatabaseConnectionViewModel();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
