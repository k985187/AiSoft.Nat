using System;
using System.Collections.Generic;
using System.Net;
using AiSoft.Nat.Base;
using AiSoft.Nat.Enums;

namespace AiSoft.Nat.Upnp.Messages.Requests
{
	internal class CreatePortMappingRequestMessage : RequestMessageBase
	{
		private readonly Mapping _mapping;

		public CreatePortMappingRequestMessage(Mapping mapping)
		{
			_mapping = mapping;
		}

		public override IDictionary<string, object> ToXml()
		{
			var remoteHost = _mapping.PublicIP.Equals(IPAddress.None) ? string.Empty : _mapping.PublicIP.ToString();

			return new Dictionary<string, object>
					   {
						   {"NewRemoteHost", remoteHost},
						   {"NewExternalPort", _mapping.PublicPort},
						   {"NewProtocol", _mapping.Protocol == Protocol.Tcp ? "TCP" : "UDP"},
						   {"NewInternalPort", _mapping.PrivatePort},
						   {"NewInternalClient", _mapping.PrivateIP},
						   {"NewEnabled", 1},
						   {"NewPortMappingDescription", _mapping.Description},
						   {"NewLeaseDuration", _mapping.Lifetime}
					   };
		}
	}
}