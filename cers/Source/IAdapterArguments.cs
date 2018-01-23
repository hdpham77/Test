using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace CERS.EDT
{
	public interface IAdapterArguments
	{
		NameValueCollection ToNameValueCollection();
	}
}