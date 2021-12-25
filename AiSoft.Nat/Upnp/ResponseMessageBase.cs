using System;
using System.Xml;

namespace AiSoft.Nat.Upnp
{
	internal abstract class ResponseMessageBase
	{
		private readonly XmlDocument _document;
		protected string ServiceType;
		private readonly string _typeName;

		protected ResponseMessageBase(XmlDocument response, string serviceType, string typeName)
		{
			_document = response;
			ServiceType = serviceType;
			_typeName = typeName;
		}

		protected XmlNode GetNode()
		{
			var nsm = new XmlNamespaceManager(_document.NameTable);
			nsm.AddNamespace("responseNs", ServiceType);

			var typeName = _typeName;
			var messageName = typeName.Substring(0, typeName.Length - "Message".Length);
			var node = _document.SelectSingleNode("//responseNs:" + messageName, nsm);
            if (node == null)
            {
                throw new InvalidOperationException("The response is invalid: " + messageName);
            }
            return node;
		}
	}
}