using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CERS.EDT.Windows.Client
{
	public class WindowBase : Window
	{
		#region Fields

		private OutputPanel _OutputPanel;
		private BackgroundWorker _Worker;

		#endregion Fields

		#region Properties

		public Brush InputErrorStyle
		{
			get
			{
				return new LinearGradientBrush( Colors.LightPink, Colors.LightSalmon, 90 );
			}
		}

		public Brush InputStyle
		{
			get
			{
				return new SolidColorBrush( Colors.White );
			}
		}

		public BackgroundWorker Worker
		{
			get
			{
				if ( _Worker == null )
				{
					InitBackgroundWorker();
				}
				return _Worker;
			}
		}

		#endregion Properties

		#region InitBackgroundWorker Method

		protected virtual void InitBackgroundWorker()
		{
			_Worker = new BackgroundWorker();
			_Worker.WorkerReportsProgress = true;
			_Worker.DoWork += new DoWorkEventHandler( _Worker_DoWork );
			_Worker.ProgressChanged += new ProgressChangedEventHandler( _Worker_ProgressChanged );
			_Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler( _Worker_RunWorkerCompleted );
		}

		#endregion InitBackgroundWorker Method

		#region RunInBackground Method(s)

		protected virtual void RunInBackground( BackgroundOperationType operationType )
		{
			if ( _Worker != null && !_Worker.IsBusy )
			{
				BackgroundOperationArgs args = new BackgroundOperationArgs( operationType );
				RunInBackground( args );
			}
		}

		protected virtual void RunInBackground<T>( T args ) where T : BackgroundOperationArgs
		{
			if ( _Worker != null && !_Worker.IsBusy )
			{
				_Worker.RunWorkerAsync( args );
			}
		}

		protected virtual void RunInBackground<T>( BackgroundOperationType operationType, T args )
		{
			if ( _Worker != null && !_Worker.IsBusy )
			{
				BackgroundOperationArgs<T> opArgs = new BackgroundOperationArgs<T>( args, operationType );

				_Worker.RunWorkerAsync( opArgs );
			}
		}

		#endregion RunInBackground Method(s)

		#region BackgroundWorker Events

		private void _Worker_DoWork( object sender, DoWorkEventArgs e )
		{
			DoWork( e );
		}

		private void _Worker_ProgressChanged( object sender, ProgressChangedEventArgs e )
		{
			ProgressChanged( e );
		}

		private void _Worker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
		{
			RunWorkerCompleted( e );
		}

		#endregion BackgroundWorker Events

		#region Background Worker Methods

		protected virtual void DoWork( DoWorkEventArgs e )
		{
		}

		protected virtual void ProgressChanged( ProgressChangedEventArgs e )
		{
		}

		protected virtual void RunWorkerCompleted( RunWorkerCompletedEventArgs e )
		{
		}

		#endregion Background Worker Methods

		#region SetSelectedItem Method

		public virtual void SetSelectedItem( ListView listView )
		{
			listView.SelectedItem = listView.Tag;
			listView.ScrollIntoView( listView.Tag );
		}

		#endregion SetSelectedItem Method

		#region Validation Helper Methods

		#region IsNotEmpty Methods

		public bool IsNotEmpty( TextBox textBox )
		{
			bool result = !string.IsNullOrWhiteSpace( textBox.Text );
			if ( !result )
			{
				textBox.Background = InputErrorStyle;
			}
			else
			{
				textBox.Background = InputStyle;
			}
			return result;
		}

		public bool IsNotEmpty( PasswordBox passwordBox )
		{
			bool result = !string.IsNullOrWhiteSpace( passwordBox.Password );
			if ( !result )
			{
				passwordBox.Background = InputErrorStyle;
			}
			else
			{
				passwordBox.Background = InputStyle;
			}
			return result;
		}

		#endregion IsNotEmpty Methods

		#endregion Validation Helper Methods

		#region UpdateControlUsability Method

		protected virtual void UpdateControlUsability( bool enabled, params Control[] controls )
		{
			if ( controls != null )
			{
				foreach ( var control in controls )
				{
					control.IsEnabled = enabled;
				}
			}
		}

		#endregion UpdateControlUsability Method

		#region RegisterOutputPanel Method

		protected virtual void RegisterOutputPanel( OutputPanel op )
		{
			_OutputPanel = op;
		}

		#endregion RegisterOutputPanel Method

		#region UpdateEndpointUrlFromWorkerThread Method

		protected virtual void UpdateEndpointUrl( StringBuilder endpointUrl, bool fromWorkerThread = true )
		{
			UpdateEndpointUrl( endpointUrl.ToString(), fromWorkerThread );
		}

		protected virtual void UpdateEndpointUrl( string endpointUrl, bool fromWorkerThread = true )
		{
			Action<string> updater = parm =>
			{
				if ( _OutputPanel != null )
				{
					_OutputPanel.EndpointUrl = parm;
				}
			};

			if ( fromWorkerThread )
			{
				Dispatcher.Invoke( updater, endpointUrl );
			}
			else
			{
				updater( endpointUrl );
			}
		}

		#endregion UpdateEndpointUrlFromWorkerThread Method

		#region UpdateStatusFromWorkerThread

		protected virtual void UpdateStatus( string status, bool fromWorkerThread = true )
		{
			Action<string> updater = parm =>
			{
				if ( _OutputPanel != null )
				{
					_OutputPanel.Status = status;
				}
			};

			if ( fromWorkerThread )
			{
				Dispatcher.Invoke( updater, status );
			}
			else
			{
				updater( status );
			}
		}

		#endregion UpdateStatusFromWorkerThread

		protected virtual void UpdateOutputPanel<T>( T result, bool fromWorkerThread = true ) where T : RestClientResult
		{
			Action<RestClientResult> updater = restClientResult =>
			{
				if ( _OutputPanel != null )
				{
					_OutputPanel.Update( restClientResult );
				}
			};

			if ( fromWorkerThread )
			{
				Dispatcher.Invoke( updater, result );
			}
			else
			{
				updater( result );
			}
		}
	}
}