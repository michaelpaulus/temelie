using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Cornerstone.Database.UI.Views;

/// <summary>
/// Interaction logic for Main.xaml
/// </summary>
public partial class Main : UserControl
{

    public Main()
    {
        // This call is required by the designer.
        InitializeComponent();

        // Add any initialization after the InitializeComponent() call.
        foreach (var menuItem in (
            from i in this.Menu.Items.OfType<MenuItem>()
            select i))
        {
            menuItem.Header = menuItem.Header.ToString().ToLower();
        }

    }

    #region Methods

    private void ShowPage(Type type, string title)
    {
        var element = (Control)Activator.CreateInstance(type);

        this.TitleLabel.Content = title.ToUpper();

        this.ContentFrame.Children.Clear();

        this.ContentFrame.Children.Add(element);
    }

    #endregion

    #region Event Handlers

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        var menuItem = (MenuItem)sender;
        var type = System.Type.GetType("Cornerstone.Database." + menuItem.CommandParameter.ToString());
        this.ShowPage(type, menuItem.Header.ToString());
    }

    #endregion

}
