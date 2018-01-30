using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Controls;
using System.Xml.Linq;

namespace CERS.EDT.Windows.Client
{
	public static class ExtensionMethods
	{
		#region ToInt32 Method

		public static int ToInt32( this XElement element, int defaultValue = 0 )
		{
			int result = defaultValue;
			if ( element != null )
			{
				if ( !string.IsNullOrWhiteSpace( element.Value ) )
				{
					int temp = 0;
					if ( int.TryParse( element.Value, out temp ) )
					{
						result = temp;
					}
				}
			}
			return result;
		}

		#endregion ToInt32 Method

		#region ToNullableInt32 Method

		public static int? ToNullableInt32( this XElement element, int? defaultValue = null )
		{
			int? result = defaultValue;
			if ( element != null )
			{
				if ( !string.IsNullOrWhiteSpace( element.Value ) )
				{
					int temp = 0;
					if ( int.TryParse( element.Value, out temp ) )
					{
						result = temp;
					}
				}
			}
			return result;
		}

		#endregion ToNullableInt32 Method

		#region ToDateTime Method

		public static DateTime? ToDateTime( this XElement element, DateTime? defaultValue = null )
		{
			DateTime? result = defaultValue;
			if ( element != null )
			{
				if ( !string.IsNullOrWhiteSpace( element.Value ) )
				{
					DateTime temp;
					if ( DateTime.TryParse( element.Value, out temp ) )
					{
						result = temp;
					}
				}
			}
			return result;
		}

		#endregion ToDateTime Method

		#region GetChildElementValue Method

		public static T GetChildElementValue<T>( this XElement parentElement, string elementName, T defaultValue = default(T) )
		{
			T result = defaultValue;

			if ( string.IsNullOrWhiteSpace( elementName ) )
			{
				throw new ArgumentNullException( "elementName" );
			}

			if ( parentElement != null )
			{
				var element = parentElement.Element( elementName );
				if ( element != null )
				{
					result = element.ToTypedValue<T>( defaultValue );
				}
			}

			return result;
		}

		#endregion GetChildElementValue Method

		#region ToTypedValue Method

		public static T ToTypedValue<T>( this XElement element, T defaultValue = default(T) )
		{
			T result = defaultValue;
			if ( element != null )
			{
				string rawValue = element.Value;
				if ( !string.IsNullOrWhiteSpace( rawValue ) )
				{
					result = ChangeType<T>( rawValue );
				}
			}
			return result;
		}

		#endregion ToTypedValue Method

		#region ChangeType Method

		public static object ChangeType( object value, Type conversionType )
		{
			if ( conversionType == null )
			{
				throw new ArgumentNullException( "conversionType" );
			}

			if ( conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals( typeof( Nullable<> ) ) )
			{
				if ( value == null || value == DBNull.Value )
				{
					return null;
				}

				//NullableConverter converter = new NullableConverter(conversionType);
				//conversionType == converter.UnderlyingType;
				conversionType = Nullable.GetUnderlyingType( conversionType );
			}
			return value.GetType().Equals( conversionType ) == true ? value : TypeDescriptor.GetConverter( conversionType ).ConvertFrom( value );

			//return Convert.ChangeType(value, conversionType);
		}

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static T ChangeType<T>( object value )
		{
			return (T) ChangeType( value, typeof( T ) );
		}

		#endregion ChangeType Method

		#region AddQueryString Method(s)

		public static void AddQueryString( this StringBuilder builder, bool? propertyValue, string name )
		{
			if ( propertyValue != null )
			{
				if ( builder.Length > 0 )
				{
					builder.Append( "&" );
				}
				builder.Append( name ).Append( "=" ).Append( HttpUtility.UrlEncode( propertyValue.Value.ToString() ) );
			}
		}

		public static void AddQueryString( this StringBuilder builder, DateTime? propertyValue, string name )
		{
			if ( propertyValue != null )
			{
				if ( builder.Length > 0 )
				{
					builder.Append( "&" );
				}
				builder.Append( name ).Append( "=" ).Append( HttpUtility.UrlEncode( propertyValue.Value.ToShortDateString() ) );
			}
		}

		public static void AddQueryString( this StringBuilder builder, string propertyValue, string name )
		{
			if ( !string.IsNullOrWhiteSpace( propertyValue ) )
			{
				if ( builder.Length > 0 )
				{
					builder.Append( "&" );
				}
				builder.Append( name ).Append( "=" ).Append( HttpUtility.UrlEncode( propertyValue ) );
			}
		}

		public static void AddQueryString( this StringBuilder builder, int? propertyValue, string name )
		{
			if ( propertyValue.HasValue )
			{
				if ( builder.Length > 0 )
				{
					builder.Append( "&" );
				}
				builder.Append( name ).Append( "=" ).Append( HttpUtility.UrlEncode( propertyValue.Value.ToString() ) );
			}
		}

		public static void AddQueryString( this StringBuilder builder, bool propertyValue, string name )
		{
			if ( builder.Length > 0 )
			{
				builder.Append( "&" );
			}
			builder.Append( name ).Append( "=" ).Append( HttpUtility.UrlEncode( propertyValue.ToString() ) );
		}

		#endregion AddQueryString Method(s)

		public static void Add<TValue>( this List<SelectableValue<TValue>> items, string text, TValue value, bool selected = false )
		{
			SelectableValue<TValue> item = new SelectableValue<TValue>
			{
				Text = text,
				Value = value,
				Selected = selected
			};
			items.Add( item );
		}

		public static string GetSelectedValues<TValue>( this IEnumerable<SelectableValue<TValue>> items, string delimiter = "," )
		{
			StringBuilder result = new StringBuilder();
			var selectedItems = ( from r in items where r.Selected select r.Value ).ToList();
			for ( int index = 0; index <= selectedItems.Count - 1; index++ )
			{
				result.Append( selectedItems[index] );
				if ( index < selectedItems.Count - 1 )
				{
					result.Append( delimiter );
				}
			}
			return result.ToString();
		}

		public static string GetString( this byte[] data, Encoding encoding = null )
		{
			if ( encoding == null )
			{
				encoding = Encoding.UTF8;
			}

			string result = null;
			if ( data != null )
			{
				result = encoding.GetString( data );
			}
			return result;
		}

		public static XElement GetXElement( this byte[] data, Encoding encoding = null )
		{
			string xmlString = data.GetString( encoding );
			Debug.WriteLine( xmlString );
			XElement result = XElement.Parse( xmlString );
			return result;
		}

		public static int? ToInt32( this TextBox textBox )
		{
			int? result = null;

			if ( !string.IsNullOrWhiteSpace( textBox.Text ) )
			{
				int temp = 0;
				if ( int.TryParse( textBox.Text, out temp ) )
				{
					result = temp;
				}
			}

			return result;
		}

		#region WriteToFile Method(s)

		public static void WriteToFile( this byte[] data, string fileName )
		{
			Utility.WriteToFile( data, fileName );
		}

		public static void WriteToFile( this string data, string fileName )
		{
			Utility.WriteToFile( data, fileName );
		}

		#endregion WriteToFile Method(s)
	}
}