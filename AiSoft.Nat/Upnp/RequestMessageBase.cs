using System;
using System.Collections.Generic;

namespace AiSoft.Nat.Upnp
{
	internal abstract class RequestMessageBase
	{
		public abstract IDictionary<string, object> ToXml();
	}
}