using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
	/// Interaction logic for OutputPanel.xaml
	/// </summary>
	public partial class OutputPanel : UserControl
	{
		private RestClientResult _LastRestClientResult = null;

		public string EndpointUrl
		{
			get { return tbEndpointUrl.Text; }
			set { tbEndpointUrl.Text = value; }
		}

		public string OutputText
		{
			get { return tbOutput.Text; }
			set { tbOutput.Text = value; }
		}

		public string Status
		{
			get { return tbStatus.Text; }
			set { tbStatus.Text = value; }
		}

		public void Update(RestClientResult result)
		{
			if (result != null)
			{
				_LastRestClientResult = result;
				tbEndpointUrl.Text = result.EndpointUrl;

				tbStatus.Text = result.Status.ToString() + " (HTTP " + ((int)result.Status) + ")";
				if (result.Status.ToString() != result.StatusDescription)
				{
					tbStatus.Text += ": " + result.StatusDescription;
				}

				if (result.Status == HttpStatusCode.OK)
				{
					tbStatus.Foreground = new SolidColorBrush(Colors.DarkGreen);
				}
				else if (result.Status == HttpStatusCode.InternalServerError)
				{
					tbStatus.Foreground = new SolidColorBrush(Colors.Red);
				}
				else
				{
					tbStatus.Foreground = new SolidColorBrush(Colors.Orange);
				}

				tbContentLength.Text = result.ContentLength.ToString();
				tbContentType.Text = result.ContentType;

				if (result.RawData != null)
				{
					if (result.ContentType.Contains("text/xml"))
					{
						try
						{
							var xElement = result.RawData.GetXElement();
							tbOutput.Text = xElement.ToString();
						}
						catch (Exception ex)
						{
							tbOutput.Text = "[Error: Couldn't deserialize the data into an XElement.\r\nSpecific Error: " + ex.Message + "\r\n]\r\n\r\n";
							tbOutput.Text += result.RawData.GetString();
						}
					}
				}

				if (result.Error)
				{
					tbException.Text = result.Exception.Message;
				}
				else
				{
					tbException.Text = "N/A";
				}
			}
		}

		public OutputPanel()
		{
			InitializeComponent();
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			if (_LastRestClientResult != null)
			{
				if (_LastRestClientResult.ContentType.Contains("text/xml"))
				{
					Utility.SaveFile(Utility.GetParentWindow(this), tbOutput.Text);
				}
				else if (_LastRestClientResult.ContentType.Contains("text/pdf"))
				{
					Utility.SaveFile(Utility.GetParentWindow(this), tbOutput.Text, "Save PDF File", "PDF Files|*.pdf");
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(tbOutput.Text))
				{
					Utility.SaveFile(Utility.GetParentWindow(this), tbOutput.Text, "Save File", "TXT Files|*.txt");
				}
			}
		}

		private void btnCopy_Click(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(tbOutput.Text))
			{
				Clipboard.SetText(tbOutput.Text);
			}
		}

		private void btnClear_Click(object sender, RoutedEventArgs e)
		{
			tbOutput.Text = "";
		}
	}
}