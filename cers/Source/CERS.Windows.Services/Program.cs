using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace CERS.Windows.Services
{
	internal static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		private static void Main()
		{
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
				new HMIDeferredProcessingProcessService(),
				new FacInfoDeferredProcessingProcessService(),
                new SubmittalDeltaProcessService(),
                new PrintJobProcessService(),
                new JobDocumentCleanUpProcessService(),
                new DeferredJobProcessService(),
			};
			ServiceBase.Run( ServicesToRun );
		}
	}
}