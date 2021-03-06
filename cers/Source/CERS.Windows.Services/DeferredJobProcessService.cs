﻿using CERS.ServiceProcesses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CERS.Windows.Services
{
    partial class DeferredJobProcessService : ServiceBase
    {
        private DeferredJobService _Process;

        public DeferredJobProcessService()
        {
            InitializeComponent();
            RegenerateServiceName();
            _Process = new DeferredJobService();
        }

        private void RegenerateServiceName()
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

        protected override void OnStart( string[] args )
        {
            _Process.Start();
        }

        protected override void OnStop()
        {
            _Process.Stop();
        }
    }
}
