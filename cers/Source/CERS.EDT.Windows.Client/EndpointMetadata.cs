using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CERS.EDT.Windows.Client
{
	public class EndpointMetadata
	{
		public string Acronym { get; set; }

		public string Category { get; set; }

		public string CategoryDescription { get; set; }

		public string Comments { get; set; }

		public string Description { get; set; }

		public SchemaInfo InputSchema { get; set; }

		public string Name { get; set; }

		public SchemaInfo OutputSchema { get; set; }

		public string Status { get; set; }

		public int Tier { get; set; }

		public string Uri { get; set; }

		public string UrlFragment { get; set; }

		public static EndpointMetadata GetByAcronym( string acroynm )
		{
			EndpointMetadata result = null;
			try
			{
				RestClient rc = new RestClient( App.AuthorizationHeader );
				var rcResult = rc.ExecuteXml( Endpoints.Endpoint_EndpointMetadataQuery );
				if ( rcResult.Status == HttpStatusCode.OK )
				{
					var xml = rcResult.Element;
					var element = ( from x in xml.Descendants( "Endpoint" ) where x.Element( "Acronym" ).Value == acroynm select x ).FirstOrDefault();
					if ( element != null )
					{
						result = new EndpointMetadata();
						result.Name = element.GetChildElementValue( "Name", "" );
						result.Acronym = element.GetChildElementValue( "Acronym", "" );
						result.Tier = element.GetChildElementValue<int>( "Tier", 1 );
						result.Description = element.GetChildElementValue( "Description", "" );
						result.Comments = element.GetChildElementValue( "Comments", "" );
						result.Status = element.GetChildElementValue( "Status", "" );
						result.Category = element.GetChildElementValue( "Category", "" );
						result.CategoryDescription = element.GetChildElementValue( "CategoryDescription", "" );
						result.UrlFragment = element.GetChildElementValue( "UrlFragment", "" );
						result.Uri = element.GetChildElementValue( "Uri", "" );

						var isXml = element.Element( "InputSchema" );
						if ( isXml != null )
						{
							result.InputSchema = new SchemaInfo();
							result.InputSchema.Name = isXml.GetChildElementValue( "Name", "" );
							result.InputSchema.Location = isXml.GetChildElementValue( "Location", "" );
							result.InputSchema.Namespace = isXml.GetChildElementValue( "Namespace", "" );
							result.InputSchema.PublishDate = isXml.GetChildElementValue<DateTime>( "PublishDate", DateTime.Now );
							result.InputSchema.FileName = isXml.GetChildElementValue( "FileName", "" );
						}

						var oxXml = element.Element( "OutputSchema" );
						if ( oxXml != null )
						{
							result.OutputSchema = new SchemaInfo();
							result.OutputSchema.Name = oxXml.GetChildElementValue( "Name", "" );
							result.OutputSchema.Location = oxXml.GetChildElementValue( "Location", "" );
							result.OutputSchema.Namespace = oxXml.GetChildElementValue( "Namespace", "" );
							result.OutputSchema.PublishDate = oxXml.GetChildElementValue<DateTime>( "PublishDate", DateTime.Now );
							result.OutputSchema.FileName = oxXml.GetChildElementValue( "FileName", "" );
						}
					}
				}
			}
			catch
			{
			}

			return result;
		}

		public class SchemaInfo
		{
			public string FileName { get; set; }

			public string Location { get; set; }

			public string Name { get; set; }

			public string Namespace { get; set; }

			public DateTime PublishDate { get; set; }
		}
	}
}