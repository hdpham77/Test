using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using CERS.ServiceProcesses;

namespace CERS.Windows.Services
{
	partial class FacInfoDeferredProcessingProcessService : ServiceBase
	{
		private FacilityInformationDeferredProcessingService _Process;

		public FacInfoDeferredProcessingProcessService()
		{
			InitializeComponent();
			RegenerateServiceName();
			_Process = new FacilityInformationDeferredProcessingService();
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

		protected override void OnStart(string[] args)
		{
			_Process.Start();
		}

		protected override void OnStop()
		{
			_Process.Stop();
		}
	}
}