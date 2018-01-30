namespace CERS.Windows.Services
{
	partial class ProjectInstaller
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.spi = new System.ServiceProcess.ServiceProcessInstaller();
			this.siHMIDeferredProcessingService = new System.ServiceProcess.ServiceInstaller();
			this.siFacInfoDeferredProcessingService = new System.ServiceProcess.ServiceInstaller();
			this.siSubmittalDeltaProcessService = new System.ServiceProcess.ServiceInstaller();
			this.siPrintJobProcessService = new System.ServiceProcess.ServiceInstaller();
			this.siJobDocumentCleanUpProcessingService = new System.ServiceProcess.ServiceInstaller();
			this.siDeferredJobProcessService = new System.ServiceProcess.ServiceInstaller();
			// 
			// spi
			// 
			this.spi.Password = null;
			this.spi.Username = null;
			// 
			// siHMIDeferredProcessingService
			// 
			this.siHMIDeferredProcessingService.DelayedAutoStart = true;
			this.siHMIDeferredProcessingService.Description = "Processes the HMI Deferred Processing Queue";
			this.siHMIDeferredProcessingService.DisplayName = "CERS HMI Deferred Processing Service";
			this.siHMIDeferredProcessingService.ServiceName = "CERSHMIDeferredProcessingService";
			this.siHMIDeferredProcessingService.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			// 
			// siFacInfoDeferredProcessingService
			// 
			this.siFacInfoDeferredProcessingService.DelayedAutoStart = true;
			this.siFacInfoDeferredProcessingService.Description = "Processes Owner/Operator deferred owner operator uploads.";
			this.siFacInfoDeferredProcessingService.DisplayName = "CERS Owner/Information Deferred Processing Service";
			this.siFacInfoDeferredProcessingService.ServiceName = "CERSFacInfoDeferredProcessingService";
			this.siFacInfoDeferredProcessingService.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			// 
			// siSubmittalDeltaProcessService
			// 
			this.siSubmittalDeltaProcessService.DelayedAutoStart = true;
			this.siSubmittalDeltaProcessService.Description = "Service that will process Queued Submittal Comparisons";
			this.siSubmittalDeltaProcessService.DisplayName = "CERS Submittal Delta Process Service";
			this.siSubmittalDeltaProcessService.ServiceName = "SubmittalDeltaProcessService";
			this.siSubmittalDeltaProcessService.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			// 
			// siPrintJobProcessService
			// 
			this.siPrintJobProcessService.DelayedAutoStart = true;
			this.siPrintJobProcessService.Description = "Processes print jobs";
			this.siPrintJobProcessService.DisplayName = "CERS Print Job Process Service";
			this.siPrintJobProcessService.ServiceName = "CERSPrintJobProcessService";
			this.siPrintJobProcessService.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			// 
			// siJobDocumentCleanUpProcessingService
			// 
			this.siJobDocumentCleanUpProcessingService.DelayedAutoStart = true;
			this.siJobDocumentCleanUpProcessingService.Description = "Purging Job Physical Document";
			this.siJobDocumentCleanUpProcessingService.DisplayName = "CERS Job Document Clean Up Process Service";
			this.siJobDocumentCleanUpProcessingService.ServiceName = "CERSJobDocumentCleanUpProcessService";
			this.siJobDocumentCleanUpProcessingService.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			// 
			// siDeferredJobProcessService
			// 
			this.siDeferredJobProcessService.DelayedAutoStart = true;
			this.siDeferredJobProcessService.Description = "Processing Deferred Jobs";
			this.siDeferredJobProcessService.DisplayName = "CERS DeferredJobProcessService";
			this.siDeferredJobProcessService.ServiceName = "CERSDeferredJobProcessService";
			this.siDeferredJobProcessService.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.spi,
            this.siHMIDeferredProcessingService,
            this.siFacInfoDeferredProcessingService,
            this.siSubmittalDeltaProcessService,
            this.siPrintJobProcessService,
            this.siJobDocumentCleanUpProcessingService,
            this.siDeferredJobProcessService});

		}

		#endregion

		private System.ServiceProcess.ServiceProcessInstaller spi;
		private System.ServiceProcess.ServiceInstaller siHMIDeferredProcessingService;
		private System.ServiceProcess.ServiceInstaller siFacInfoDeferredProcessingService;
        private System.ServiceProcess.ServiceInstaller siSubmittalDeltaProcessService;
        private System.ServiceProcess.ServiceInstaller siPrintJobProcessService;
		private System.ServiceProcess.ServiceInstaller siJobDocumentCleanUpProcessingService;
        private System.ServiceProcess.ServiceInstaller siDeferredJobProcessService;
	}
}