using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CERS.EDT.Windows.Client
{
	/// <summary>
	/// Interaction logic for EnvironmentSelectorWindow.xaml
	/// </summary>
	public partial class EnvironmentSelectorWindow : WindowBase
	{
		public EnvironmentSelectorWindow()
		{
			InitializeComponent();
			Title = AssemblyMetadata.Title + ": Select Environment";
			cboEnvironment.ItemsSource = App.Environments;
			cboEnvironment.DisplayMemberPath = "Key";
			cboEnvironment.SelectedValuePath = "Key";
			cboEnvironment.SelectedIndex = 0;
		}

		private void btnClose_Click( object sender, RoutedEventArgs e )
		{
			App.Current.Shutdown();
		}

		private void btnSelect_Click( object sender, RoutedEventArgs e )
		{
			if ( cboEnvironment.SelectedItem != null )
			{
				App.CurrentEnvironment = (TargetEnvironment) cboEnvironment.SelectedValue;
				this.Hide();
				LoginWindow window = new LoginWindow();
				window.Owner = this;
				window.Show();
			}
		}
	}
}