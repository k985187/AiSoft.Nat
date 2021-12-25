using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AiSoft.Nat.Base;
using AiSoft.Nat.Exceptions;
using AiSoft.Nat.Utils;

namespace AiSoft.Nat.Upnp
{
	internal class SoapClient
	{
		private readonly string _serviceType;
		private readonly Uri _url;

		public SoapClient(Uri url, string serviceType)
		{
			_url = url;
			_serviceType = serviceType;
		}

        public async Task<XmlDocument> InvokeAsync(string operationName, IDictionary<string, object> args)
		{
			NatDiscoverer.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "SOAPACTION: **{0}** url:{1}", operationName, _url);
			var messageBody = BuildMessageBody(operationName, args);
			var request = BuildHttpWebRequest(operationName, messageBody);
            if (messageBody.Length > 0)
			{
				using (var stream = await request.GetRequestStreamAsync())
				{
					await stream.WriteAsync(messageBody, 0, messageBody.Length);
				}
			}
            using(var response = await GetWebResponse(request))
			{
				var stream = response.GetResponseStream();
				var contentLength = response.ContentLength;
                var reader = new StreamReader(stream, Encoding.UTF8);
                var responseBody = contentLength != -1 ? reader.ReadAsMany((int) contentLength) : reader.ReadToEnd();
                var responseXml = GetXmlDocument(responseBody);
                response.Close();
				return responseXml;
			}
		}

		private static async Task<WebResponse> GetWebResponse(WebRequest request)
		{
			WebResponse response;
			try
			{
				response = await request.GetResponseAsync();
			}
			catch (WebException ex)
			{
				NatDiscoverer.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "WebException status: {0}", ex.Status);
                response = ex.Response as HttpWebResponse;
                if (response == null)
                {
                    throw;
                }
			}
			return response;
		}

        private HttpWebRequest BuildHttpWebRequest(string operationName, byte[] messageBody)
		{
            var request = WebRequest.CreateHttp(_url);
            request.KeepAlive = false;
			request.Method = "POST";
			request.ContentType = "text/xml; charset=\"utf-8\"";
			request.Headers.Add("SOAPACTION", "\"" + _serviceType + "#" + operationName + "\"");
			request.ContentLength = messageBody.Length;
			return request;
		}

		private byte[] BuildMessageBody(string operationName, IEnumerable<KeyValuePair<string, object>> args)
		{
			var sb = new StringBuilder();
			sb.AppendLine("<s:Envelope ");
			sb.AppendLine("   xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" ");
			sb.AppendLine("   s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">");
			sb.AppendLine("   <s:Body>");
			sb.AppendLine("	  <u:" + operationName + " xmlns:u=\"" + _serviceType + "\">");
			foreach (var a in args)
			{
				sb.AppendLine("		 <" + a.Key + ">" + Convert.ToString(a.Value, CultureInfo.InvariantCulture) +
							  "</" + a.Key + ">");
			}
			sb.AppendLine("	  </u:" + operationName + ">");
			sb.AppendLine("   </s:Body>");
			sb.Append("</s:Envelope>\r\n\r\n");
			var requestBody = sb.ToString();
            var messageBody = Encoding.UTF8.GetBytes(requestBody);
			return messageBody;
		}

		private XmlDocument GetXmlDocument(string response)
		{
			XmlNode node;
			var doc = new XmlDocument();
			doc.LoadXml(response);
            var nsm = new XmlNamespaceManager(doc.NameTable);
            nsm.AddNamespace("errorNs", "urn:schemas-upnp-org:control-1-0");
            if ((node = doc.SelectSingleNode("//errorNs:UPnPError", nsm)) != null)
			{
				var code = Convert.ToInt32(node.GetXmlElementText("errorCode"), CultureInfo.InvariantCulture);
				var errorMessage = node.GetXmlElementText("errorDescription");
				NatDiscoverer.TraceSource.LogWarn("Server failed with error: {0} - {1}", code, errorMessage);
				throw new MappingException(code, errorMessage);
			}
            return doc;
		}
	}
}