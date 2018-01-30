using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CERS.EDT.Windows.Client
{
	public class Utility
	{
		#region GetParentWindow Method

		public static Window GetParentWindow( DependencyObject child )
		{
			DependencyObject parentObject = VisualTreeHelper.GetParent( child );

			if ( parentObject == null )
			{
				return null;
			}

			Window parent = parentObject as Window;
			if ( parent != null )
			{
				return parent;
			}
			else
			{
				return GetParentWindow( parentObject );
			}
		}

		#endregion GetParentWindow Method

		#region SaveFile Method

		public static void SaveFile( Window owner, string data, string title = "Save XML File", string filter = "XML Files|*.xml", bool promptAutoOpen = true )
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Title = title;
			sfd.Filter = filter + "|All Files|*.*";
			if ( sfd.ShowDialog() == true )
			{
				try
				{
					data.WriteToFile( sfd.FileName );
					if ( promptAutoOpen )
					{
						if ( MessageBox.Show( owner, "Would you like to view the file downloaded?", "CERS EDT Windows Client", MessageBoxButton.YesNo, MessageBoxImage.Question ) == MessageBoxResult.Yes )
						{
							Process.Start( new ProcessStartInfo( sfd.FileName ) );
						}
					}
				}
				catch ( Exception ex )
				{
					MessageBox.Show( owner, ex.Message, "CERS EDT Windows Client", MessageBoxButton.OK, MessageBoxImage.Error );
				}
			}
		}

		public static void SaveFile( Window owner, byte[] data, string title = "Save XML File", string filter = "XML Files|*.xml", bool promptAutoOpen = true )
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Title = title;
			sfd.Filter = filter + "|All Files|*.*";
			if ( sfd.ShowDialog() == true )
			{
				try
				{
					data.WriteToFile( sfd.FileName );

					if ( promptAutoOpen )
					{
						if ( MessageBox.Show( owner, "Would you like to view the file downloaded?", "CERS EDT Windows Client", MessageBoxButton.YesNo, MessageBoxImage.Question ) == MessageBoxResult.Yes )
						{
							Process.Start( new ProcessStartInfo( sfd.FileName ) );
						}
					}
				}
				catch ( Exception ex )
				{
					MessageBox.Show( owner, ex.Message, "CERS EDT Windows Client", MessageBoxButton.OK, MessageBoxImage.Error );
				}
			}
		}

		#endregion SaveFile Method

		#region OpenFile Method

		public static string OpenFile( Window owner, string title = "Select XML File", string filter = "XML Files|*.xml" )
		{
			string result = null;
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Title = title;
			ofd.Filter = filter + "|ZIP Files|*.zip|All Files|*.*";
			if ( ofd.ShowDialog( owner ) == true )
			{
				result = ofd.FileName;
			}
			return result;
		}

		#endregion OpenFile Method

		public static byte[] ReadBinaryFile( string fileName )
		{
			byte[] data = null;
			using ( FileStream stream = new FileStream( fileName, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				data = new byte[stream.Length];
				stream.Read( data, 0, (int) stream.Length );
				stream.Close();
			}
			return data;
		}

		public static string ReadTextFile( string fileName )
		{
			string result = string.Empty;
			using ( StreamReader reader = new StreamReader( fileName ) )
			{
				result = reader.ReadToEnd();
				reader.Close();
			}
			return result;
		}

		public static string SelectFolder( string defaultPath = "" )
		{
			string result = defaultPath;
			System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
			if ( dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK )
			{
				result = dlg.SelectedPath;
			}

			return result;
		}

		public static void WriteToFile( byte[] data, string fileName )
		{
			if ( !string.IsNullOrWhiteSpace( fileName ) && data != null )
			{
				try
				{
					using ( FileStream stream = new FileStream( fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read ) )
					{
						stream.Write( data, 0, data.Length );
						stream.Close();
					}
				}
				catch ( Exception ex )
				{
					throw new Exception( "Unable to save the content to:\r\n" + fileName + "\r\n\r\n" + ex.Message, ex );
				}
			}
		}

		public static void WriteToFile( string data, string fileName )
		{
			if ( !string.IsNullOrWhiteSpace( fileName ) && data != null )
			{
				try
				{
					using ( FileStream stream = new FileStream( fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read ) )
					{
						byte[] buffer = Encoding.UTF8.GetBytes( data );
						stream.Write( buffer, 0, buffer.Length );
						stream.Close();
					}
				}
				catch ( Exception ex )
				{
					throw new Exception( "Unable to save the content to:\r\n" + fileName + "\r\n\r\n" + ex.Message, ex );
				}
			}
		}
	}
}