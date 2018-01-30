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
	/// Interaction logic for InputPanel.xaml
	/// </summary>
	public partial class InputPanel : UserControl
	{
		private bool _ShowVersion = false;

		public InputPanel()
		{
			InitializeComponent();
		}

		public bool AllowVersion
		{
			get
			{
				return _ShowVersion;
			}
			set
			{
				_ShowVersion = value;
			}
		}

		public string InputFile
		{
			get { return tbFile.Text; }
			set { tbFile.Text = value; }
		}

		public InputDataMode InputMode
		{
			get
			{
				InputDataMode result = InputDataMode.Text;
				if ( rbBinary.IsChecked == true )
				{
					result = InputDataMode.BinaryFile;
				}
				return result;
			}
		}

		public string InputText
		{
			get { return tbInput.Text; }
			set { tbInput.Text = value; }
		}

		public string VersionIdentifier
		{
			get { return tbVersionArg.Text; }
		}

		private void btnBrowse_Click( object sender, RoutedEventArgs e )
		{
			string targetFile = Utility.OpenFile( Utility.GetParentWindow( this ) );
			if ( !string.IsNullOrWhiteSpace( targetFile ) )
			{
				tbFile.Text = targetFile;
				string ext = System.IO.Path.GetExtension( targetFile ).Replace( ".", "" );
				if ( ext.Contains( "txt" ) || ext.Contains( "xml" ) )
				{
					tbInput.Text = Utility.ReadTextFile( targetFile );
					rbText.IsChecked = true;
				}
				else
				{
					rbBinary.IsChecked = true;
				}
			}
		}

		private void btnClear_Click( object sender, RoutedEventArgs e )
		{
			tbInput.Text = "";
			tbFile.Text = "";
		}

		private void btnPaste_Click( object sender, RoutedEventArgs e )
		{
			tbInput.Text = Clipboard.GetText();
		}

		private void UserControl_Loaded( object sender, RoutedEventArgs e )
		{
			System.Windows.Visibility visiblity = _ShowVersion ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
			lbVersionArgLabel.Visibility = visiblity;
			lbVersionArgExample.Visibility = visiblity;
			tbVersionArg.Visibility = visiblity;
		}
	}
}