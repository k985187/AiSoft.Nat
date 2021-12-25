using System;
using System.Collections.Generic;

namespace AiSoft.Nat.Upnp.Messages.Requests
{
	internal class GetExternalIPAddressRequestMessage : RequestMessageBase
	{
		public override IDictionary<string, object> ToXml()
		{
			return new Dictionary<string, object>();
		}
	}
}