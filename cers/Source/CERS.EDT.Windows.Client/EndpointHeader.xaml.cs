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
	/// Interaction logic for EndpointHeader.xaml
	/// </summary>
	public partial class EndpointHeader : UserControl
	{
		public EndpointHeader()
		{
			InitializeComponent();
		}

		public string Acronym { get; set; }

		public string Description
		{
			get { return tbDescription.Text; }
			set { tbDescription.Text = value; }
		}

		public string Title
		{
			get { return tbTitle.Text; }
			set { tbTitle.Text = value; }
		}

		private void hlMoreInfo_RequestNavigate( object sender, RequestNavigateEventArgs e )
		{
			Process.Start( new ProcessStartInfo( e.Uri.AbsoluteUri ) );
			e.Handled = true;
		}

		private void UserControl_Loaded( object sender, RoutedEventArgs e )
		{
			if ( !string.IsNullOrWhiteSpace( Acronym ) )
			{
				EndpointMetadata em = EndpointMetadata.GetByAcronym( Acronym );
				if ( em != null )
				{
					Title = em.Name + " (" + em.Acronym + ")";
					Description = em.Description;

					var moreInfoUrl = Endpoints.ServerBase + "/Home/EndpointDetail/" + em.Acronym;
					hlMoreInfo.NavigateUri = new Uri( moreInfoUrl );
				}
			}
		}
	}
}