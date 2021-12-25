using System;
using System.Collections.Generic;
using AiSoft.Nat.Base;
using AiSoft.Nat.Enums;

namespace AiSoft.Nat.Upnp.Messages.Requests
{
	internal class DeletePortMappingRequestMessage : RequestMessageBase
	{
		private readonly Mapping _mapping;

		public DeletePortMappingRequestMessage(Mapping mapping)
		{
			_mapping = mapping;
		}

		public override IDictionary<string, object> ToXml()
		{
			return new Dictionary<string, object>
					   {
						   {"NewRemoteHost", string.Empty},
						   {"NewExternalPort", _mapping.PublicPort},
						   {"NewProtocol", _mapping.Protocol == Protocol.Tcp ? "TCP" : "UDP"}
					   };
		}
	}
}