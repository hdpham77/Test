using System;
using System.Collections.Generic;
using System.IO;
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

namespace CERS.Windows.DocumentClone
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private CERSEntities _DataModel;

		public MainWindow()
		{
			InitializeComponent();
		}

		public CERSEntities DataModel
		{
			get
			{
				if ( _DataModel == null )
				{
					_DataModel = new CERSEntities();
				}
				return _DataModel;
			}
		}

		private void AppendStatus( string message, bool newLine = true )
		{
			tbStatus.Text += message;
			if ( newLine )
			{
				tbStatus.Text += "\r\n";
			}
		}

		private void btnClose_Click( object sender, RoutedEventArgs e )
		{
			App.Current.Shutdown();
		}

		private void btnMove_Click( object sender, RoutedEventArgs e )
		{
			//find all the documents.

			if ( !string.IsNullOrWhiteSpace( tbFacilitySubmittalID.Text ) )
			{
				int facilitySubmittalID = 0;
				if ( int.TryParse( tbFacilitySubmittalID.Text, out facilitySubmittalID ) )
				{
					List<string> sourceDocumentsVirtualPaths = new List<string>();

					FindDocuments( facilitySubmittalID, sourceDocumentsVirtualPaths );

					CopyDocuments( sourceDocumentsVirtualPaths );
				}
			}
		}

		private void ClearStatus()
		{
			tbStatus.Text = string.Empty;
		}

		private void CopyDocuments( List<string> sourceFilesVirtualPaths )
		{
			AppendStatus( "Found " + sourceFilesVirtualPaths.Count + " Documents." );
			string fileName = null;
			string directoryVirtualPath = null;
			string sourcePhysicalPath = null;
			string targetPhysicalPath = null;

			foreach ( string sourceFileVirtualPath in sourceFilesVirtualPaths )
			{
				fileName = System.IO.Path.GetFileName( sourceFileVirtualPath );
				directoryVirtualPath = System.IO.Path.GetDirectoryName( sourceFileVirtualPath );
				try
				{
					sourcePhysicalPath = tbSourceStoragePath.Text + sourceFileVirtualPath;
					if ( File.Exists( sourcePhysicalPath ) )
					{
						Directory.CreateDirectory( tbTargetStoragePath.Text + directoryVirtualPath );

						targetPhysicalPath = tbTargetStoragePath.Text + sourceFileVirtualPath;
						File.Copy( sourcePhysicalPath, targetPhysicalPath );
						AppendStatus( fileName + " copied." );
					}
					else
					{
						AppendStatus( fileName + " does not exist." );
					}
				}
				catch ( Exception ex )
				{
					AppendStatus( fileName + " was not processed. " + ex.Message );
				}
			}
		}

		private void FindDocuments( int facilitySubmittalID, List<string> sourceDocumentsVirtualPaths )
		{
			var docs = DataModel.vFacilitySubmittalElementDocumentManifests.Where( p => p.FacilitySubmittalID == facilitySubmittalID );
			foreach ( var doc in docs )
			{
				if ( !string.IsNullOrWhiteSpace( doc.Location ) )
				{
					sourceDocumentsVirtualPaths.Add( doc.Location );
				}
			}
		}

		private void FindDocumentsOld( int facilitySubmittalID, List<string> sourceDocumentsVirtualPaths )
		{
			var facilitySubmittalElements = DataModel.FacilitySubmittalElements.Where( p => p.FacilitySubmittalID == facilitySubmittalID && !p.Voided );

			foreach ( var fse in facilitySubmittalElements )
			{
				foreach ( var fser in fse.Resources.Where( p => !p.Voided ) )
				{
					foreach ( var fserd in fser.Documents.Where( p => !p.Voided ) )
					{
						foreach ( var fsed in fserd.FacilitySubmittalElementDocuments )
						{
							if ( fsed.Document != null )
							{
								sourceDocumentsVirtualPaths.Add( fsed.Document.Location );
							}
						}
					}
				}
			}
		}
	}
}