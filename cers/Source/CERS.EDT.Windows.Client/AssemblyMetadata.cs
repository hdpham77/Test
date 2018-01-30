using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CERS.EDT.Windows.Client
{
	public class AssemblyMetadata
	{
		public static string Title
		{
			get
			{
				var attribute = GetAssemblyAttribute<AssemblyTitleAttribute>();
				string result = "Unknown";
				if ( attribute != null )
				{
					result = attribute.Title;
				}

				return result;
			}
		}

		public static T GetAssemblyAttribute<T>() where T : Attribute
		{
			Type type = typeof( AssemblyMetadata );
			Assembly assembly = type.Assembly;

			T result = null;
			object[] attributes = assembly.GetCustomAttributes( typeof( T ), false );
			if ( attributes.Length > 0 )
			{
				result = attributes[0] as T;
			}
			return result;
		}
	}
}