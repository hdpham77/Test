using CERS.EDT.Configuration;
using CERS.Model;
using CERS.Xml;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UPF;
using UPF.Core;

namespace CERS.EDT
{
	/// <summary>
	/// The base class that all EDTProcessors derive from.
	/// </summary>
	public abstract class Adapter : IAdapter
	{
		#region Constants

		public const string DataXmlFile = "data.xml";
		public const string FileNameElement = "FileName";

		#endregion Constants

		#region Fields

		private CERSEDTConfigurationSection _Config;

		#endregion Fields

		#region Properties

		public int ContextAccountID { get { return Repository.ContextAccountID; } }

		public CERSEDTConfigurationSection EDTConfig
		{
			get
			{
				if ( _Config == null )
				{
					_Config = CERSEDTConfigurationSection.Current;
				}
				return _Config;
			}
		}

		/// <summary>
		/// Gets the <see cref="EDTTransactionScope"/> associated with this <see cref="Adapter"/>.
		/// </summary>
		public EDTTransactionScope TransactionScope { get; protected set; }

		protected ICERSRepositoryManager Repository
		{
			get
			{
				return TransactionScope.Repository;
			}
		}

		protected ICERSSystemServiceManager ServiceManager
		{
			get
			{
				return ServiceLocator.GetSystemServiceManager( Repository );
			}
		}

		#endregion Properties

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Adapter"/> class when given a <paramref name="transactionScope"/>.
		/// </summary>
		/// <param name="transactionScope">The <see cref="EDTTransactionScope"/> to initialize the <see cref="Adapter"/> with.</param>
		public Adapter( EDTTransactionScope transactionScope )
		{
			if ( transactionScope == null )
			{
				throw new ArgumentNullException( "transactionScope", "The specified TransactionScope is null, and is required." );
			}
			TransactionScope = transactionScope;
		}

		#endregion Constructors

		#region Events

		/// <summary>
		/// Event raised when a NotificationMessage is received from the <see cref="Adapter"/>.
		/// </summary>
		public event EventHandler<EDTProcessorNotificationEventArgs> NotificationReceived;

		#endregion Events

		#region Protected Methods

		#region ValidateXml Method

		/// <summary>
		/// Validates the specified <paramref name="xDoc"/> against the specified <paramref name="schema"/>.
		/// </summary>
		/// <param name="xDoc">The <see cref="XDocument"/> of the XML to validate.</param>
		/// <param name="schema">The <see cref="XmlSchema"/> to validate the <paramref name="xDoc"/> against.</param>
		/// <returns></returns>
		protected virtual bool ValidateXml( XDocument xDoc, XmlSchema schema )
		{
			bool result = true;
			string xml = xDoc.ToString();
			TransactionScope.WriteActivity( "XML Document is validating..." );

            //get the root element of the doc
            var rootElement = xDoc.Root;

            //try and obtain the targetNamespace
            var targetNamespace = rootElement.GetDefaultNamespace();
            if ( string.IsNullOrWhiteSpace( targetNamespace.NamespaceName ) )
            {
                //couldn't obtain the schema we need because the root node didn't have a default non-prefixed xmlns attribute.
                //lets see of the document is using prefixes and look for the xmlns:cers="" attribute.
                targetNamespace = rootElement.GetNamespaceOfPrefix( "cers" );
            }

            //if we have no targetNamespace then we should not proceed any further.
            if ( targetNamespace == null || targetNamespace.NamespaceName == string.Empty )
            {
                TransactionScope.WriteMessage( "Unable to resolve the targetNamespace of the received XML data.", EDTTransactionMessageType.Error );
                throw new Exception( "Unable to resolve the targetNamespace of the received XML data." );
            }

            //check the version
            string version = targetNamespace.ExtractVersion();

            XmlSchemaValidationResult schemaValidationResult = xml.IsValidAgainstSchema( schema, version );
			if ( !schemaValidationResult.IsValid )
			{
				//package is invalid due to schema validation errors, update the transaction status write some info.
				//Merge in the schema validation error messages and associate them with the transaction.

				TransactionScope.SetStatus( EDTTransactionStatus.Rejected );
				TransactionScope.WriteActivity( "XML Document is NOT valid against the schema." );
				TransactionScope.WriteMessage( "XML document does not validate against the schema.", EDTTransactionMessageType.Error );
				foreach ( var error in schemaValidationResult.Errors )
				{
					TransactionScope.WriteMessage( "Schema Validation Error: " + error, EDTTransactionMessageType.Error );
				}

				result = false;
			}
			else
			{
				TransactionScope.WriteActivity( "XML Document is valid against the schema." );
			}
			return result;
		}

		#endregion ValidateXml Method

		#region OnNotificationReceived Method

		/// <summary>
		/// Raise the <see cref="NotificationReceived"/> event.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="userData"></param>
		protected virtual void OnNotificationReceived( string message, object userData = null )
		{
			if ( NotificationReceived != null )
			{
				NotificationReceived( this, new EDTProcessorNotificationEventArgs( message, userData ) );
			}
		}

		#endregion OnNotificationReceived Method

		#region IsZipPackage Method

		public bool IsZipPackage( Stream dataStream )
		{
			return IOHelper.IsValidZip( dataStream );
		}

		#endregion IsZipPackage Method

		#region ConvertStreamToXDocument Method

		/// <summary>
		/// Converts a Text Stream or ZIP File Stream into an XDocument instance.  If an XML instance
		/// is not found, or the XML does not pass schema validation, a value of "null" is returned.
		/// </summary>
		/// <param name="edtDataStream"></param>
		/// <param name="edtDataFlow"></param>
		/// <returns></returns>
		protected virtual XDocument ConvertStreamToXDocument( Stream edtDataStream )
		{
			XDocument xDocument = null;

			// Check to see if data was sent as an XML File
			if ( IOHelper.IsWellFormedXML( edtDataStream ) )
			{
				edtDataStream.Position = 0;
				string xml = IOHelper.ReadToEnd( edtDataStream );
				xDocument = XDocument.Parse( xml );
			}

			return xDocument;
		}

		#endregion ConvertStreamToXDocument Method

		#region ValidateZip Method

		/// <summary>
		/// Runs 3 different kinds of validation.
		/// 1 - Checks to see if the zip contains a Data.xml file
		/// 2 - Checks to see if Zip contains and directories
		/// 3 - Checks to see if all document in xml are present in the file.
		/// </summary>
		/// <param name="zipData">The <see cref="System.Byte"/> array of the Zip file to verify.</param>
		/// <returns>A <see cref="System.Boolean"/> value indicating whether the validation passed.</returns>
		public bool ValidateZip( byte[] zipData )
		{
			ZipFile zip = ZipFile.Read( zipData );
			return ValidateZip( zip );
		}

		/// <summary>
		/// Runs 3 different kinds of validation.
		/// 1 - Checks to see if the zip contains a Data.xml file
		/// 2 - Checks to see if Zip contains and directories
		/// 3 - Checks to see if all document in xml are present in the file.
		/// </summary>
		/// <param name="stream">The <see cref="System.IO.Stream"/> of data containing the zip file to validate.</param>
		/// <returns>A <see cref="System.Boolean"/> value indicating whether the validation passed.</returns>
		public bool ValidateZip( Stream stream )
		{
			ZipFile zip = ZipFile.Read( stream );
			return ValidateZip( zip );
		}

		/// <summary>
		/// Runs 3 different kinds of validation.
		/// 1 - Checks to see if the zip contains a Data.xml file
		/// 2 - Checks to see if Zip contains and directories
		/// 3 - Checks to see if all document in xml are present in the file.
		/// </summary>
		/// <param name="zip">The <see cref="ZipFile"/> to verify.</param>
		/// <returns>A <see cref="System.Boolean"/> value indicating whether the validation passed.</returns>
		public bool ValidateZip( ZipFile zip )
		{
			bool result = false;

			try
			{
				if ( !DoesZipContainFile( zip, DataXmlFile ) )
				{
					throw new Exception( "Could not find the expected XML file (" + DataXmlFile + ") in the zip file." );
				}

				if ( DoesZipContainSubDirectories( zip ) )
				{
					throw new Exception( "The zip file contains sub directories which is not allowed." );
				}

				if ( !DocumentsNotReferencedInXmlButContainedInZip( zip, DataXmlFile ) )
				{
					throw new Exception( "The Zip file contains documents that are not referenced in the XML." );
				}

				if ( !DocumentsReferencedInXmlButNotContainedInZip( zip, DataXmlFile ) )
				{
					throw new Exception( "The XML data references documents not contained in the Zip which is not allowed." );
				}

				if ( !IsXmlWellFormedInZip( zip ) )
				{
					throw new Exception( "The XML file (" + DataXmlFile + ") in the zip file is not well formed XML." );
				}

				result = true;
			}
			catch ( Exception ex )
			{
				if ( TransactionScope != null )
				{
					TransactionScope.WriteMessage( ex.Message, EDTTransactionMessageType.Required );
				}
				result = false;
			}

			return result;
		}

		#endregion ValidateZip Method

		#region ReturnXDocumentFromZip Method

		/// <summary>
		/// Returns the XElement associated with "data.xml" in the provided ZIP file.
		/// </summary>
		/// <param name="zip"></param>
		/// <param name="schemaLocation"></param>
		/// <returns></returns>
		public XDocument ReturnXDocumentFromZip( byte[] zipData )
		{
			XDocument xDocument = null;

			try
			{
				ZipFile zip = ZipFile.Read( zipData );
				MemoryStream ms = new MemoryStream();
				ZipEntry e = zip[DataXmlFile];
				e.Extract( ms );
				ms.Position = 0;
				xDocument = XDocument.Load( ms );
			}
			catch
			{
				xDocument = null;
			}

			return xDocument;
		}

		#endregion ReturnXDocumentFromZip Method

		#region DoesZipContainFile Method

		/// <summary>
		/// Returns true if the Zip package contains a file called data.xml
		/// </summary>
		/// <param name="zip"></param>
		/// <param name="xmlFileName"></param>
		/// <returns></returns>
		protected virtual bool DoesZipContainFile( ZipFile zip, string xmlFileName = DataXmlFile )
		{
			return zip.ContainsFile( xmlFileName );
		}

		#endregion DoesZipContainFile Method

		#region DoesZipContainSubDirectories Method

		/// <summary>
		/// Returns true if the zip file contains subdirectories
		/// </summary>
		/// <param name="zip"></param>
		/// <returns></returns>
		protected virtual bool DoesZipContainSubDirectories( ZipFile zip )
		{
			return zip.DoesZipContainSubDirectories();
		}

		#endregion DoesZipContainSubDirectories Method

		#region GetDocumentsNotReferencedInXmlButContainedInZip Method

		protected virtual List<string> GetDocumentsNotReferencedInXmlButContainedInZip( ZipFile zip, string xmlFileName = DataXmlFile )
		{
			//get the list of filenames referenced as attachments in the XML file.
			var documentsInXml = ExtractDocumentFileNamesFromXml( zip, xmlFileName );

			//get the list of filenames in the zip.
			List<string> documentsInZip = zip.EntryFileNames.ToList();

			//remove the xmlFileName from the list, which would leave us only attachment filenames intended to be compared against the XML.
			documentsInZip.Remove( xmlFileName );

			//get list of documents not referenced in XML but contained in zip.
			var documentsNotInXml = documentsInZip.Except( documentsInXml );

			return documentsNotInXml.ToList();
		}

		#endregion GetDocumentsNotReferencedInXmlButContainedInZip Method

		#region GetDocumentsReferencedInXmlButNotContainedInZip Method

		protected virtual List<string> GetDocumentsReferencedInXmlButNotContainedInZip( ZipFile zip, string xmlFileName = DataXmlFile )
		{
			//get the list of filenames referenced as attachments in the XML file.
			var documentsInXml = ExtractDocumentFileNamesFromXml( zip, xmlFileName );

			//get the list of filenames in the zip.
			List<string> documentsInZip = zip.EntryFileNames.ToList();

			//remove the xmlFileName from the list, which would leave us only attachment filenames intended to be compared against the XML.
			documentsInZip.Remove( xmlFileName );

			//find documents that did not exist in the zip that were referenced by the XML.
			var documentsNotInZip = documentsInXml.Except( documentsInZip );

			return documentsNotInZip.ToList();
		}

		#endregion GetDocumentsReferencedInXmlButNotContainedInZip Method

		#region DocumentsReferencedInXmlButNotContainedInZip Method

		protected virtual bool DocumentsReferencedInXmlButNotContainedInZip( ZipFile zip, string xmlFileName = DataXmlFile )
		{
			return ( GetDocumentsReferencedInXmlButNotContainedInZip( zip, xmlFileName ).Count == 0 );
		}

		#endregion DocumentsReferencedInXmlButNotContainedInZip Method

		#region DocumentsNotReferencedInXmlButContainedInZip Method

		protected virtual bool DocumentsNotReferencedInXmlButContainedInZip( ZipFile zip, string xmlFileName = DataXmlFile )
		{
			return ( GetDocumentsNotReferencedInXmlButContainedInZip( zip, xmlFileName ).Count == 0 );
		}

		#endregion DocumentsNotReferencedInXmlButContainedInZip Method

		#region ExtractXmlFile Method

		protected virtual XDocument ExtractXmlFile( ZipFile zip, string xmlFileName = DataXmlFile )
		{
			return zip.ExtractXmlFile( xmlFileName );
		}

		#endregion ExtractXmlFile Method

		#region ExtractDocumentFileNamesFromXml Method

		protected virtual List<string> ExtractDocumentFileNamesFromXml( ZipFile zip, string xmlFileName = DataXmlFile )
		{
			var xDoc = ExtractXmlFile( zip, xmlFileName );
			return ExtractDocumentFileNamesFromXml( xDoc );
		}

		#endregion ExtractDocumentFileNamesFromXml Method

		#region ExtractDocumentFileNamesFromXml Method

		protected virtual List<string> ExtractDocumentFileNamesFromXml( XDocument xdoc )
		{
			List<string> result = new List<string>();
			var documents = xdoc.Root.Descendants().Where( d => d.Name.LocalName.EndsWith( FileNameElement ) ).ToList();

			if ( documents.Count > 0 )
			{
				foreach ( var document in documents )
				{
					result.Add( document.Value.Clean() );
				}
			}

			return result;
		}

		#endregion ExtractDocumentFileNamesFromXml Method

		#region IsXmlValidInZip Method

		/// <summary>
		/// Return true if the XMl in the ZipFile is well formed and is validated against the schema
		/// </summary>
		/// <param name="zip"></param>
		/// <returns></returns>
		protected virtual bool IsXmlValidInZip( ZipFile zip, string schemaLocation )
		{
			bool result = true;

			try
			{
				MemoryStream ms = new MemoryStream();
				ZipEntry e = zip[DataXmlFile];
				e.Extract( ms );
				ms.Position = 0;
				XDocument xdoc = XDocument.Load( ms );
				var schemaValidationResult = xdoc.ToString().IsValidAgainstSchema( schemaLocation );
				result = schemaValidationResult.IsValid;
			}
			catch
			{
				result = false;
			}

			return result;
		}

		#endregion IsXmlValidInZip Method

		#region IsXmlWellFormedInZip Method

		/// <summary>
		/// Return true if the XMl in the ZipFile is well formed.
		/// </summary>
		/// <param name="zip"></param>
		/// <returns></returns>
		protected virtual bool IsXmlWellFormedInZip( ZipFile zip )
		{
			bool result = true;

			try
			{
				MemoryStream ms = new MemoryStream();
				ZipEntry e = zip[DataXmlFile];
				e.Extract( ms );
				ms.Position = 0;
				XDocument xdoc = XDocument.Load( ms );
			}
			catch
			{
				result = false;
			}

			return result;
		}

		#endregion IsXmlWellFormedInZip Method

		#endregion Protected Methods
	}
}