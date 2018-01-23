using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.EDT
{
	public class EDTProcessorNotificationEventArgs : EventArgs
	{
		public string Message { get; set; }

		public object UserData { get; set; }

		public EDTProcessorNotificationEventArgs(string message, object userData = null)
		{
			Message = message;
			UserData = userData;
		}
	}
}