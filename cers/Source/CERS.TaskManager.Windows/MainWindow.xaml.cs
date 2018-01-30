using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using CERS.Plugins;
using UPF;

namespace CERS.TaskManager.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Fields

		private BackgroundWorker _Worker;

		#endregion Fields

		#region Properties

		public List<PluginDefinitionSelection> AvailablePlugins { get; set; }

		#endregion Properties

		public MainWindow()
		{
			InitializeComponent();
			InitBackgroundWorker();
		}

		#region Control Events

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			AvailablePlugins = Utilities.GetAvailablePlugins();
			lvPlugins.ItemsSource = AvailablePlugins;
		}

		private void btnExit_Click(object sender, RoutedEventArgs e)
		{
			App.Current.Shutdown();
		}

		private void btnRun_Click(object sender, RoutedEventArgs e)
		{
			BeginRun();
		}

		#endregion Control Events

		#region BeginRun Method

		private void BeginRun()
		{
			int selectedPluginCount = AvailablePlugins.Count(p => p.Selected);
			if (selectedPluginCount == 0)
			{
				MessageBox.Show(this, "No plug-in(s) were selected to run. Please select at least one plug-in to run.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}
			lvMessages.Items.Clear();
			btnRun.IsEnabled = false;
			lvPlugins.IsEnabled = false;
			_Worker.RunWorkerAsync();
		}

		#endregion BeginRun Method

		#region Run Method

		private void Run()
		{
			AddPluginMessageFromThread("Begin Run, executing plug-ins...", "Host Application");

			var plugins = from r in AvailablePlugins where r.Selected select r;
			foreach (var plugin in plugins)
			{
				Utilities.RunPlugin(plugin, thePlugin_Notification, thePlugin_ProgressChanged);
			}

			AddPluginMessageFromThread("Run completed, all plug-ins executed.", "Host Application");
		}

		#endregion Run Method

		#region Plugin Events

		private void thePlugin_ProgressChanged(object sender, PluginProgressChangedEventArgs e)
		{
			_Worker.ReportProgress(e.ProgressPercentage);
		}

		private void thePlugin_Notification(object sender, PluginNotificationEventArgs e)
		{
			AddPluginMessageFromThread(e.Message, e.PluginName, e.Error, e.Exception);
		}

		#endregion Plugin Events

		#region AddPluginMessageFromThread Methods

		private void AddPluginMessageFromThread(string message, string plugin, bool error = false, Exception ex = null)
		{
			PluginMessage pm = new PluginMessage();
			pm.Message = message;
			if (ex != null)
			{
				pm.Message += " Exception Detail: " + ex.Format();
			}
			pm.PluginName = plugin;
			pm.Received = DateTime.Now;
			pm.Error = error;
			AddPluginMessageFromThread(pm);
		}

		private void AddPluginMessageFromThread(PluginMessage message)
		{
			Action<PluginMessage> method = pm =>
			{
				//to help keep memory down, lets not let more than 500 items go into the list.
				if (lvMessages.Items.Count >= 500)
				{
					lvMessages.Items.Clear();
				}

				lvMessages.Items.Add(pm);
				lvMessages.Tag = pm;
				SetSelectedItem(lvMessages);
				lblPluginMessage.Content = pm.PluginName + " (" + pm.ReceivedDisplay + "): " + ((pm.Error) ? "ERROR! " : "") + pm.Message;
			};

			Dispatcher.Invoke(method, message);
		}

		#endregion AddPluginMessageFromThread Methods

		#region BackgroundWorker Events

		private void _Worker_DoWork(object sender, DoWorkEventArgs e)
		{
			Run();
		}

		private void _Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			btnRun.IsEnabled = true;
			lvPlugins.IsEnabled = true;
			pbStatus.Value = 0;
		}

		private void _Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			pbStatus.Value = e.ProgressPercentage;
		}

		#endregion BackgroundWorker Events

		#region SetSelectedItem Method

		public virtual void SetSelectedItem(ListView listView)
		{
			listView.SelectedItem = listView.Tag;
			listView.ScrollIntoView(listView.Tag);
		}

		#endregion SetSelectedItem Method

		#region InitBackgroundWorker Method

		private void InitBackgroundWorker()
		{
			_Worker = new BackgroundWorker();
			_Worker.WorkerReportsProgress = true;
			_Worker.DoWork += new DoWorkEventHandler(_Worker_DoWork);
			_Worker.ProgressChanged += new ProgressChangedEventHandler(_Worker_ProgressChanged);
			_Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_Worker_RunWorkerCompleted);
		}

		#endregion InitBackgroundWorker Method

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Button button = sender as Button;
			if (button != null)
			{
				if (button.Parent != null)
				{
					//MessageBox.Show(button.Parent.ToString());
					Grid grid = (Grid)button.Parent;
					if (grid != null)
					{
						foreach (var child in grid.Children)
						{
							if (child.GetType() == typeof(TextBox))
							{
								TextBox textbox = (TextBox)child;
								if (textbox != null)
								{
									Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
									dialog.Multiselect = false;
									dialog.Title = "Choose Input File";
									if (dialog.ShowDialog(this) == true)
									{
										textbox.Text = dialog.FileName;
									}
								}
								break;
							}
						}
					}
				}
			}
		}

		private void lvPlugins_Loaded(object sender, RoutedEventArgs e)
		{
		}

		private void Button_Loaded(object sender, RoutedEventArgs e)
		{
			Button button = sender as Button;
			TextBox textbox = null;
			TextBlock textBlock = null;
			if (button != null)
			{
				if (button.Parent != null)
				{
					Grid grid = (Grid)button.Parent;
					if (grid != null)
					{
						foreach (var child in grid.Children)
						{
							if (child.GetType() == typeof(TextBox))
							{
								textbox = (TextBox)child;
							}
							else if (child.GetType() == typeof(TextBlock))
							{
								textBlock = (TextBlock)child;
							}
						}
					}
				}
			}

			if (textbox != null && button != null && textBlock != null)
			{
				bool enabled = (textBlock.Text == "Yes");
				textbox.IsEnabled = enabled;
				button.IsEnabled = enabled;
			}
		}
	}
}