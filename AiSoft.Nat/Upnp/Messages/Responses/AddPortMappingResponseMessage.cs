using System;
using System.Xml;

namespace AiSoft.Nat.Upnp.Messages.Responses
{
    internal class AddPortMappingResponseMessage : ResponseMessageBase
    {
        public AddPortMappingResponseMessage(XmlDocument response, string serviceType)
            : base(response, serviceType, "AddPortMappingResponseMessage")
        {
        }
    }
}