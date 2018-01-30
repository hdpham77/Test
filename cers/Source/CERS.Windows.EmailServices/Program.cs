using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CERS.Windows.EmailServices
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
				new EmailQueueProcessService()
			};
			ServiceBase.Run( ServicesToRun );
		}
	}
}