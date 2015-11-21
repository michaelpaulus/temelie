
using System.Linq;
using System.Xml.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using DatabaseTools.Extensions;

namespace DatabaseTools
{
	public partial class Main
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
			SubscribeToEvents();
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

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			this.SetPlacement(Configuration.Preferences.UserSettingsContext.Current.MainWindowPlacement);
		}

		private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Configuration.Preferences.UserSettingsContext.Current.MainWindowPlacement = this.GetPlacement();
			Configuration.Preferences.UserSettingsContext.Save();
		}

#endregion



		private bool EventsSubscribed = false;
		private void SubscribeToEvents()
		{
			if (EventsSubscribed)
				return;
			else
				EventsSubscribed = true;

			this.Closing += Main_Closing;
		}

	}

}