using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using UPF;

namespace CERS.Windows.Services
{
	[RunInstaller( true )]
	public partial class ProjectInstaller : System.Configuration.Install.Installer
	{
		public ProjectInstaller()
		{
			InitializeComponent();
			RegenerateServiceNames();
		}

		protected virtual void RegenerateServiceNames()
		{
#if DEVELOPMENT

			RegenerateServicesNames(
				RuntimeEnvironment.Development
			);

#elif PRODUCTION
			RegenerateServicesNames(
				RuntimeEnvironment.Production
			);
#elif STAGING
			RegenerateServicesNames(
				RuntimeEnvironment.Staging
			);
#elif TRAINING
			RegenerateServicesNames(
				RuntimeEnvironment.Training
			);
#elif TESTING
            RegenerateServicesNames(
                RuntimeEnvironment.Testing
            );
#elif DOCUMENTATION
			RegenerateServicesNames(
				RuntimeEnvironment.Documentation
			);
#endif
		}

		protected virtual void RegenerateServicesNames( RuntimeEnvironment runtimeEnvironment )
		{
			RegenerateServicesNamesForServices(
				runtimeEnvironment,
				siHMIDeferredProcessingService,
				siFacInfoDeferredProcessingService,
				siSubmittalDeltaProcessService,
				siPrintJobProcessService,
				siJobDocumentCleanUpProcessingService,
				siDeferredJobProcessService
			);
		}

		protected virtual void RegenerateServicesNamesForServices( RuntimeEnvironment runtimeEnvironment, params ServiceInstaller[] serviceInstallers )
		{
			foreach ( ServiceInstaller serviceInstaller in serviceInstallers )
			{
				serviceInstaller.ServiceName += "_" + runtimeEnvironment.ToString().ToUpper();
				serviceInstaller.Description += " (" + runtimeEnvironment.ToString() + " Environment)";
				serviceInstaller.DisplayName += " (" + runtimeEnvironment.ToString() + ")";
			}
		}
	}
}