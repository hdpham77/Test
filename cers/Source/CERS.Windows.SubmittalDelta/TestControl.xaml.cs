using CERS;
using CERS.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Xml.Linq;
using UPF;
using UPF.Core;
using UPF.Core.Cryptography;
using UPF.Core.Model;

namespace CERS.Windows.SubmittalDelta
{
	/// <summary>
	/// Interaction logic for TestControl.xaml
	/// </summary>
	public partial class TestControl : UserControl
	{
		public TestControl()
		{
			InitializeComponent();
			SDFsidTB.Text = "100678";
		}

		public ICERSRepositoryManager DataRepository { get; set; }

		private void ClearButton_Click( object sender, RoutedEventArgs e )
		{
			OutputTB.Clear();
		}

		private XElement FSERHtml( XElement fser )
		{
			XElement outer = new XElement( "div" );
			IEnumerable<XElement> fields = fser.Descendants( "Field" );

			if ( fields.Count() > 0 )
			{
				XElement fieldOuter =
					new XElement( "div",
						new XElement( "h3", fser.Attribute( "ResourceType" ).Value ) );

				XElement table =
					new XElement( "table",
						new XElement( "tr",
							new XElement( "th", "Field Name" ),
							new XElement( "th", "New Value" ),
							new XElement( "th", "Old Value" ) ) );
				fieldOuter.Add( table );

				foreach ( XElement field in fields )
				{
					table.Add(
						new XElement( "tr",
							new XElement( "td", field.Attribute( "Name" ).Value ),
							new XElement( "td", field.Attribute( "NewValue" ).Value ),
							new XElement( "td", field.Attribute( "OldValue" ).Value ) ) );
				}
				outer.Add( fieldOuter );
			}

			IEnumerable<XElement> docs = fser.Descendants( "Document" );

			if ( docs.Count() > 0 )
			{
				XElement docOuter =
					new XElement( "div",
						new XElement( "h3", fser.Attribute( "ResourceType" ).Value ) );

				XElement table =
					new XElement( "table",
						new XElement( "tr",
							new XElement( "th", "Change" ),
							new XElement( "th", "Option" ),
							new XElement( "th", "Document Name" ) ) );
				docOuter.Add( table );

				foreach ( XElement doc in docs )
				{
					table.Add(
						new XElement( "tr",
							new XElement( "td", doc.Attribute( "Change" ).Value ),
							new XElement( "td", doc.Attribute( "Option" ).Value ),
							new XElement( "td", doc.Attribute( "DocumentName" ).Value ) ) );
				}
				outer.Add( docOuter );
			}

			return outer;
		}

		private XElement GenerateHtml( XElement deltaElement )
		{
			XElement fse = deltaElement.Descendants( "FSE" ).FirstOrDefault();

			XElement divMain =
				new XElement( "div",
					new XElement( "h2", fse.Attribute( "SubmittalElementType" ).Value ) );

			IEnumerable<XElement> htmlElements =
				( from fser in fse.Descendants( "FSER" )
				  select FSERHtml( fser ) );

			divMain.Add( htmlElements );
			return divMain;
		}

		private void GenerateJson( XElement deltaElement )
		{
			var fieldRows =
				from fser in deltaElement.Descendants( "FSER" )
				from field in fser.Descendants( "Field" )
				select new
				{
					fser = fser.Attribute( "ResourceType" ).Value,
					fieldName = field.Attribute( "Name" ).Value,
					newValue = field.Attribute( "NewValue" ).Value,
					oldValue = field.Attribute( "OldValue" ).Value
				};
		}

		private void InventorySummaryButton_Click( object sender, RoutedEventArgs e )
		{
			ICERSSystemServiceManager services = ServiceLocator.GetSystemServiceManager( DataRepository );

			try
			{
				int fsid = 0;
				fsid = Convert.ToInt32( ISFsidTB.Text );

				services.BusinessLogic.SubmittalElements.HazardousMaterialsInventory.InventorySummary.Summary( Convert.ToInt32( fsid ) );

				// all this was done in Summary but we need the information to get the output
				FacilitySubmittal fs = ( from fsRec in DataRepository.DataModel.FacilitySubmittals where fsRec.ID == fsid select fsRec ).FirstOrDefault();
				FacilitySubmittalElement fse =
					( from fseRec in DataRepository.DataModel.FacilitySubmittalElements
					  where fseRec.FacilitySubmittal.ID == fs.ID && fseRec.Voided == false
						  && fseRec.SubmittalElementID == (int) SubmittalElementType.HazardousMaterialsInventory
					  select fseRec ).FirstOrDefault();
				FacilitySubmittalElementResource fser =
					( from fserRec in DataRepository.DataModel.FacilitySubmittalElementResources
					  where fserRec.FacilitySubmittalElementID == fse.ID
						  && fserRec.Voided == false && fserRec.ResourceTypeID == (int) ResourceType.HazardousMaterialInventory
					  select fserRec ).FirstOrDefault();

				InventorySummary inventory = DataRepository.InventorySummaries.GetByFSERID( fser.ID );
				StringBuilder results = new StringBuilder();
				results.Append( "Inventory Summary  FSE: " + fse.ID + " CERSID: " + fse.CERSID + "\n" );

				results.Append( "   Solid Material Count: " + inventory.SolidMaterialCount + " EHS Count: " +
					inventory.SolidEHSCount + " Maximum Daily Value: " + inventory.SolidMaximumDailyValue + " pounds\n" );
				results.Append( "   Liquid Material Count: " + inventory.LiquidMaterialCount + " EHS Count: " +
					inventory.LiquidEHSCount + " Maximum Daily Value: " + inventory.LiquidMaximumDailyValue + " gallons\n" );
				results.Append( "   Gas Material Count: " + inventory.GasMaterialCount + " EHS Count: " +
					inventory.GasEHSCount + " Maximum Daily Value: " + inventory.GasMaximumDailyValue + " cubic feet\n" );
				results.Append( "   Unique Location Count: " + inventory.UniqueLocationCount + "\n" );

				this.OutputTB.Text = results.ToString();
			}
			catch ( Exception ex )
			{
				this.OutputTB.Text = "Error: " + ex.Message;
			}
		}

		private void SummaryDeltaButton_Click( object sender, RoutedEventArgs e )
		{
			ICERSSystemServiceManager services = ServiceLocator.GetSystemServiceManager( DataRepository );

			try
			{
				int fsid = 0;
				fsid = Convert.ToInt32( SDFsidTB.Text );
				XElement submittalDeltaElement = services.SubmittalDelta.DeltaFS( fsid );
				this.OutputTB.Text = submittalDeltaElement.ToString();
			}
			catch ( Exception ex )
			{
				this.OutputTB.Text = "Error: " + ex.Message;
			}
		}

		private void SummaryDeltaFSEButton_Click( object sender, RoutedEventArgs e )
		{
			ICERSSystemServiceManager services = ServiceLocator.GetSystemServiceManager( DataRepository );

			try
			{
				int fseid = 0;
				fseid = Convert.ToInt32( SDFseidTB.Text );
                XElement submittalDeltaElement = services.SubmittalDelta.ProcessDeltaFSE(fseid);
				//XElement htmlElement = GenerateHtml(submittalDeltaElement);
				//this.OutputTB.Text = htmlElement.ToString();
				//GenerateJson(submittalDeltaElement);

				this.OutputTB.Text = submittalDeltaElement.ToString();
			}
			catch ( Exception ex )
			{
				this.OutputTB.Text = "Error: " + ex.Message;
			}
		}

		private void UserControl_SizeChanged( object sender, SizeChangedEventArgs e )
		{
			if ( e.WidthChanged )
			{
				if ( e.NewSize.Width > 640 )
				{
					OutputTB.Width = e.NewSize.Width - 40;
				}
				else
				{
					OutputTB.Width = 600;
				}
			}
		}
	}
}