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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CERS.EDT.Windows.Client
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : WindowBase
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void btnCLQ_Click( object sender, RoutedEventArgs e )
		{
			CLQWindow window = new CLQWindow();
			window.Owner = this;
			window.ShowDialog();
		}

		private void btnDD_Click( object sender, RoutedEventArgs e )
		{
			DDQWindow window = new DDQWindow();
			window.Owner = this;
			window.ShowDialog();
		}

		private void btnExit_Click( object sender, RoutedEventArgs e )
		{
			App.Current.Shutdown();
		}

		private void btnRAIQ_Click( object sender, RoutedEventArgs e )
		{
			RAIQWindow window = new RAIQWindow();
			window.Owner = this;
			window.ShowDialog();
		}

		private void btnRCMEQ_Click( object sender, RoutedEventArgs e )
		{
			RCMEQWindow window = new RCMEQWindow();
			window.Owner = this;
			window.ShowDialog();
		}

		private void btnRCMES_Click( object sender, RoutedEventArgs e )
		{
			RCMESWindow window = new RCMESWindow();
			window.Owner = this;
			window.ShowDialog();
		}

		private void btnRFC_Click( object sender, RoutedEventArgs e )
		{
			RFCWindow window = new RFCWindow();
			window.Owner = this;
			window.ShowDialog();
		}

		private void btnRFM_Click( object sender, RoutedEventArgs e )
		{
			RFMWindow window = new RFMWindow();
			window.Owner = this;
			window.ShowDialog();
		}

		private void btnRFQ_Click( object sender, RoutedEventArgs e )
		{
			RFQWindow window = new RFQWindow();
			window.Owner = this;
			window.ShowDialog();
		}

		private void btnRFS_Click( object sender, RoutedEventArgs e )
		{
			RFSWindow window = new RFSWindow();
			window.Owner = this;
			window.ShowDialog();
		}

		private void btnRFSAN_Click( object sender, RoutedEventArgs e )
		{
			RFSANWindow window = new RFSANWindow();
			window.Owner = this;
			window.ShowDialog();
		}

		private void btnRFSDQ_Click( object sender, RoutedEventArgs e )
		{
			RFSDQWindow window = new RFSDQWindow();
			window.Owner = this;
			window.ShowDialog();
		}

		private void btnRFSQ_Click( object sender, RoutedEventArgs e )
		{
			RFSQWindow window = new RFSQWindow();
			window.Owner = this;
			window.ShowDialog();
		}

		private void btnRFTQ_Click( object sender, RoutedEventArgs e )
		{
			RFTQWindow window = new RFTQWindow();
			window.Owner = this;
			window.ShowDialog();
		}

		private void btnROQ_Click( object sender, RoutedEventArgs e )
		{
			ROQWindow window = new ROQWindow();
			window.Owner = this;
			window.ShowDialog();
		}

		private void btnVLQ_Click( object sender, RoutedEventArgs e )
		{
			VLQWindow window = new VLQWindow();
			window.Owner = this;
			window.ShowDialog();
		}

		private void StatusBarPanel_Loaded( object sender, RoutedEventArgs e )
		{
		}

		private void WindowBase_Closed( object sender, EventArgs e )
		{
			App.Current.Shutdown();
		}

		private void WindowBase_Loaded( object sender, RoutedEventArgs e )
		{
			this.Title = AssemblyMetadata.Title;
		}
	}
}