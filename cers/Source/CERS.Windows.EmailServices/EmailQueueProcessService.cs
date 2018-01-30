using CERS.ServiceProcesses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CERS.Windows.EmailServices
{
	public partial class EmailQueueProcessService : ServiceBase
	{
		private EmailServiceProcess _Process;

		public EmailQueueProcessService()
		{
			InitializeComponent();
			RegenerateServiceName();
			_Process = new EmailServiceProcess();
		}

		protected override void OnStart( string[] args )
		{
			_Process.Start();
		}

		protected override void OnStop()
		{
			_Process.Stop();
		}

		protected virtual void RegenerateServiceName()
		{
#if DEVELOPMENT
			this.ServiceName += "_DEVELOPMENT";
#elif PRODUCTION
			this.ServiceName += "_PRODUCTION";
#elif STAGING
			this.ServiceName += "_STAGING";
#elif TRAINING
			this.ServiceName += "_TRAINING";
#elif TESTING
			this.ServiceName += "_TESTING";
#elif DOCUMENTATION
			this.ServiceName += "_DOCUMENTATION";
#endif
		}
	}
}