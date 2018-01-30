using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.EDT.Windows.Client
{
	public class BackgroundOperationArgs
	{
		public BackgroundOperationArgs()
		{
		}

		public BackgroundOperationArgs(BackgroundOperationType type)
		{
			Type = type;
		}

		public BackgroundOperationType Type { get; set; }
	}

	public class BackgroundOperationArgs<TEndpointArgs> : BackgroundOperationArgs
	{
		public BackgroundOperationArgs(TEndpointArgs endpointArgs, BackgroundOperationType type)
			: base(type)
		{
			EndpointArguments = endpointArgs;
		}

		public TEndpointArgs EndpointArguments { get; set; }
	}
}