using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CERS.EDT.Windows.Client
{
	/// <summary>
	/// Interaction logic for StatusBarPanel.xaml
	/// </summary>
	public partial class StatusBarPanel : UserControl
	{
		public StatusBarPanel()
		{
			InitializeComponent();
		}

		private void Cersservices_RequestNavigate( object sender, RequestNavigateEventArgs e )
		{
			Process.Start( new ProcessStartInfo( e.Uri.AbsoluteUri ) );
			e.Handled = true;
		}

		private void hlSourceDownload_RequestNavigate( object sender, RequestNavigateEventArgs e )
		{
			Process.Start( new ProcessStartInfo( e.Uri.AbsoluteUri ) );
			e.Handled = true;
		}

		private void UserControl_Loaded( object sender, RoutedEventArgs e )
		{
			tbServer.Text = Endpoints.ServerBase;
			tbUsername.Text = App.Username;
			tbRegulator.Text = App.RegulatorName;
		}
	}
}