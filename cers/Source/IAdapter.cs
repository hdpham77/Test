using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.EDT
{
	public interface IAdapter
	{
		EDTTransactionScope TransactionScope { get; }

		event EventHandler<EDTProcessorNotificationEventArgs> NotificationReceived;
	}
}