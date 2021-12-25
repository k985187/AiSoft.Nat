using System;
using System.Xml;

namespace AiSoft.Nat.Upnp.Messages.Responses
{
    internal class DeletePortMappingResponseMessage : ResponseMessageBase
    {
        public DeletePortMappingResponseMessage(XmlDocument response, string serviceType)
            : base(response, serviceType, "DeletePortMappingResponseMessage")
        {
        }
    }
}