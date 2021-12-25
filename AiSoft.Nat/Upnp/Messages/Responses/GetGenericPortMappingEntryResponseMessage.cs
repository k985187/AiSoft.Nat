using System;
using System.Xml;
using AiSoft.Nat.Enums;
using AiSoft.Nat.Utils;

namespace AiSoft.Nat.Upnp.Messages.Responses
{
	internal class GetPortMappingEntryResponseMessage : ResponseMessageBase
	{
		internal GetPortMappingEntryResponseMessage(XmlDocument response, string serviceType, bool genericMapping)
			: base(response, serviceType, genericMapping ? "GetGenericPortMappingEntryResponseMessage" : "GetSpecificPortMappingEntryResponseMessage")
		{
			var data = GetNode();

			RemoteHost = (genericMapping) ? data.GetXmlElementText("NewRemoteHost") : string.Empty;
			ExternalPort = (genericMapping) ? Convert.ToInt32(data.GetXmlElementText("NewExternalPort")) : ushort.MaxValue;
            if (genericMapping)
            {
                Protocol = data.GetXmlElementText("NewProtocol").Equals("TCP", StringComparison.InvariantCultureIgnoreCase) ? Protocol.Tcp : Protocol.Udp;
            }
            else
            {
                Protocol = Protocol.Udp;
            }

			InternalPort = Convert.ToInt32(data.GetXmlElementText("NewInternalPort"));
			InternalClient = data.GetXmlElementText("NewInternalClient");
			Enabled = data.GetXmlElementText("NewEnabled") == "1";
			PortMappingDescription = data.GetXmlElementText("NewPortMappingDescription");
			LeaseDuration = Convert.ToInt32(data.GetXmlElementText("NewLeaseDuration"));
		}

		public string RemoteHost { get; private set; }

		public int ExternalPort { get; private set; }

		public Protocol Protocol { get; private set; }

		public int InternalPort { get; private set; }

		public string InternalClient { get; private set; }

		public bool Enabled { get; private set; }

		public string PortMappingDescription { get; private set; }

		public int LeaseDuration { get; private set; }
	}
}