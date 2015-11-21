
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
using System.ComponentModel;

namespace DatabaseTools
{
	namespace ViewModels
	{
		public class ViewModel : System.ComponentModel.INotifyPropertyChanged
		{
			protected virtual void OnPropertyChanged(string propertyName)
			{
				if (this.PropertyChanged != null)
				{
					PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

		}
	}


}