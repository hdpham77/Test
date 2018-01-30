using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.EDT.Windows.Client
{
	public class Regulator
	{
		public int Code { get; set; }

		public string Name { get; set; }

		public string Type { get; set; }

		public string Display
		{
			get
			{
				return Code + " - " + Name + " (" + Type + ")";
			}
		}
	}
}