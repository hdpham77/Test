using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CERS.Plugins
{
	[Plugin( "Fix FacilitySubmittalElementDocument FileName", Description = "Remove the path part of the filename as we don't want or care about it.", DeveloperName = "Mike", EnableLog = true, Order = 199 )]
	public class FixFacilitySubmittalElementDocumentFileNames : SimplePlugin
	{
		protected override void DoWork()
		{
			var fseds = from r in DataModel.FacilitySubmittalElementDocuments where !r.Voided && r.FileName.Contains( "\\" ) select r;
			int count = fseds.Count();
			int index = 0;
			string fileName = null;
			foreach ( var fsed in fseds.ToList() )
			{
				index++;
				OnNotification( "FSED: " + fsed.ID + " Original FileName: " + fsed.FileName );
				fileName = Path.GetFileName( fsed.FileName );
				if ( ( string.Compare( fileName, fsed.FileName, true ) != 0 ) && fileName.Length > 0 )
				{
					fsed.FileName = fileName;
					DataModel.SaveChanges();
					OnNotification( "Updated FSED: " + fsed.ID + " New FileName: " + fsed.FileName );
				}
				CalculateProgress( index, count );
			}
		}
	}
}