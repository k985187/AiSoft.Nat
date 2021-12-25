using System;
using System.Net;
using System.Xml;
using AiSoft.Nat.Utils;

namespace AiSoft.Nat.Upnp.Messages.Responses
{
    internal class GetExternalIPAddressResponseMessage : ResponseMessageBase
    {
        public GetExternalIPAddressResponseMessage(XmlDocument response, string serviceType)
            : base(response, serviceType, "GetExternalIPAddressResponseMessage")
        {
            var ip = GetNode().GetXmlElementText("NewExternalIPAddress");

            IPAddress ipAddr;
            if (IPAddress.TryParse(ip, out ipAddr))
            {
                ExternalIPAddress = ipAddr;
            }
        }

        public IPAddress ExternalIPAddress { get; private set; }
    }
}