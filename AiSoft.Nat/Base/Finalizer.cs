using System;
using AiSoft.Nat.Utils;

namespace AiSoft.Nat.Base
{
	sealed class Finalizer
	{
		~Finalizer()
		{
			NatDiscoverer.TraceSource.LogInfo("Closing ports opened in this session");
			NatDiscoverer.RenewTimer.Dispose();
			NatDiscoverer.ReleaseSessionMappings();
		}
	}
}