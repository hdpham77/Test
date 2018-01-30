using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.EDT.Windows.Client
{
	public class BackgroundOperationResult
	{
		public bool Success { get; set; }

		public string Message { get; set; }

		public BackgroundOperationType Type { get; set; }

		public BackgroundOperationResult()
			: this(false, BackgroundOperationType.Primary, "")
		{
		}

		public BackgroundOperationResult(bool success, BackgroundOperationType type, string message = "")
		{
			Success = success;
			Type = type;
			Message = message;
		}
	}

	public class BackgroundOperationResult<T> : BackgroundOperationResult
	{
		public BackgroundOperationResult()
			: base()
		{
		}

		public BackgroundOperationResult(bool success, BackgroundOperationType type, T data, string message = "")
			: base(success, type, message)
		{
			Data = data;
		}

		public T Data { get; set; }
	}
}