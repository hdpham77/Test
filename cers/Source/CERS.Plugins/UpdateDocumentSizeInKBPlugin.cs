using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UPF;

namespace CERS.Plugins
{
	[Plugin( "Update Document.SizeInKB", Description = "Populates the Document.SizeInKB field where it isn't populated", DeveloperName = "Mike", EnableLog = true, Order = 99 )]
	public class UpdateDocumentSizeInKBPlugin : SimplePlugin
	{
		protected override void DoWork()
		{
			//var result = DataModel.uspUpdateDocumentSizeInKB().FirstOrDefault();
			//OnNotification( "Updated " + result.FacilitySubmittalElementDocumentsUpdated + " FacilitySubmittalElementDocument related Document(s)" );
			//OnNotification( "Updated " + result.OrganizationDocumentsUpdated + " OrganizationDocument related Document(s)" );
			//OnNotification( "Updated " + result.RegulatorDocumentsUpdated + " RegulatorDocument related Document(s)" );

			var documents = DataModel.Documents.Where( p => p.SizeInKB == null && !p.Voided );
			foreach ( var document in documents )
			{
				long? sizeInBytes = DocumentStorage.GetSize( document.Location, document.StorageProviderID.Value );
				if ( sizeInBytes.HasValue )
				{
					document.SizeInKB = ( (decimal)sizeInBytes.Value / 1024 );
					Repository.Documents.Update( document );
					OnNotification( "Updated DocumentID " + document.ID + " size to " + document.SizeInKB.Value );

					foreach ( var fsed in document.FacilitySubmittalElementDocuments.Where( p => !p.Voided ).ToList() )
					{
						fsed.FileSize = (int)sizeInBytes.Value;
						Repository.FacilitySubmittalElementDocuments.Save( fsed );
					}
				}
				else
				{
					OnNotification( "DocumentID " + document.ID + " no found in storage" );
				}
			}
		}
	}
}