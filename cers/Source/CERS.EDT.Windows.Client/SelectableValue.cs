using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.EDT.Windows.Client
{
	public class SelectableValue<TValue>
	{
		public string Text { get; set; }

		public TValue Value { get; set; }

		public bool Selected { get; set; }
	}
}