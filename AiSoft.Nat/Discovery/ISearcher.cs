using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using AiSoft.Nat.Base;

namespace AiSoft.Nat.Discovery
{
	internal interface ISearcher
	{
		void Search(CancellationToken cancellationToken);

		IEnumerable<NatDevice> Receive();

		NatDevice AnalyseReceivedResponse(IPAddress localAddress, byte[] response, IPEndPoint endpoint);
	}
}