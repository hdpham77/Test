using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace CERS.EDT.Windows.Client
{
	public class RestClient
	{
		#region Properties

		public string AuthorizationHeader { get; set; }

		#endregion Properties

		#region Constructor(s)

		public RestClient()
			: this(null)
		{
		}

		public RestClient(string authorizationHeader)
		{
			AuthorizationHeader = authorizationHeader;
		}

		#endregion Constructor(s)

		#region ExecuteXml Method(s)

		public virtual RestClientXmlResult ExecuteXml(string serviceUrl, HttpMethod method = HttpMethod.Get, string contentType = "text/xml")
		{
			return ExecuteXml(serviceUrl, null, method, contentType);
		}

		public virtual RestClientXmlResult ExecuteXml(string serviceUrl, byte[] data, HttpMethod method = HttpMethod.Post, string contentType = "text/xml")
		{
			return Execute<RestClientXmlResult>(serviceUrl, data, method, contentType);
		}

		#endregion ExecuteXml Method(s)

		#region Execute Method(s)

		public virtual RestClientResult Execute(string serviceUrl, HttpMethod method = HttpMethod.Get, string contentType = "application/x-www-form-urlencoded")
		{
			return Execute<RestClientResult>(serviceUrl, null, method, contentType);
		}

		public virtual T Execute<T>(string serviceUrl, byte[] data, HttpMethod method = HttpMethod.Post, string contentType = "application/x-www-form-urlencoded") where T : RestClientResult, new()
		{
			T result = new T();

			result.EndpointUrl = serviceUrl;

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceUrl);
            // RFS's can be long
            request.Timeout = 10 * 60 * 1000;
			request.Method = method.ToString().ToUpper();
			request.ContentType = contentType;

			if (!string.IsNullOrWhiteSpace(AuthorizationHeader))
			{
				request.Headers.Add(HttpRequestHeader.Authorization, AuthorizationHeader);
			}

			if (data != null)
			{
				request.ContentLength = data.Length;
				Stream requestStream = request.GetRequestStream();
				requestStream.Write(data, 0, data.Length);
				requestStream.Close();
			}

			HttpWebResponse response = null;

			try
			{
				response = (HttpWebResponse)request.GetResponse();
				result.Status = response.StatusCode;
				result.StatusDescription = response.StatusDescription;
				result.ContentLength = response.ContentLength;
				result.ContentType = response.ContentType;

				Stream responseStream = response.GetResponseStream();

				if (result.ContentLength > 0)
				{
					result.RawData = GetBytesFromStream(responseStream);
				}
			}
            // error status like 400 will throw this exception; then we get the information from the exception
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    result.Status = ((HttpWebResponse)we.Response).StatusCode;
                    result.StatusDescription = ((HttpWebResponse)we.Response).StatusDescription;
                    result.ContentLength = we.Response.ContentLength;
                    result.ContentType = we.Response.ContentType;

                    Stream responseStream = we.Response.GetResponseStream();

                    if (result.ContentLength > 0)
                    {
                        result.RawData = GetBytesFromStream(responseStream);
                    }
                }
                else
                {
                    result.Exception = we;                    
                }
                
            }
			catch (Exception ex)
			{
				result.Exception = ex;
			}

			return result;
		}

		#endregion Execute Method(s)

		#region GetBytesFromStream Method

		protected virtual byte[] GetBytesFromStream(Stream stream)
		{
			long originalPosition = 0;

			if (stream.CanSeek)
			{
				originalPosition = stream.Position;
				stream.Position = 0;
			}

			try
			{
				byte[] readBuffer = new byte[4096];

				int totalBytesRead = 0;
				int bytesRead;

				while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
				{
					totalBytesRead += bytesRead;

					if (totalBytesRead == readBuffer.Length)
					{
						int nextByte = stream.ReadByte();
						if (nextByte != -1)
						{
							byte[] temp = new byte[readBuffer.Length * 2];
							Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
							Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
							readBuffer = temp;
							totalBytesRead++;
						}
					}
				}

				byte[] buffer = readBuffer;
				if (readBuffer.Length != totalBytesRead)
				{
					buffer = new byte[totalBytesRead];
					Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
				}
				return buffer;
			}
			finally
			{
				if (stream.CanSeek)
				{
					stream.Position = originalPosition;
				}
			}
		}

		#endregion GetBytesFromStream Method
	}
}