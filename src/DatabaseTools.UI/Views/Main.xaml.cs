using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DatabaseTools.UI.Views
{
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
            var type = System.Type.GetType("DatabaseTools." + menuItem.CommandParameter.ToString());
            this.ShowPage(type, menuItem.Header.ToString());
        }

        #endregion


    }
}
