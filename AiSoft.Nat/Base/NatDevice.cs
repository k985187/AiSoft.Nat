using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AiSoft.Nat.Enums;
using AiSoft.Nat.Utils;

namespace AiSoft.Nat.Base
{
    public abstract class NatDevice
	{
        public abstract IPEndPoint HostEndPoint { get; }

		public abstract IPAddress LocalAddress { get; }

		private readonly HashSet<Mapping> _openedMapping = new HashSet<Mapping>();

		protected DateTime LastSeen { get; private set; }

		internal void Touch()
		{
			LastSeen = DateTime.Now;
		}

		public abstract Task CreatePortMapAsync(Mapping mapping);

		public abstract Task DeletePortMapAsync(Mapping mapping);

		public abstract Task<IEnumerable<Mapping>> GetAllMappingsAsync();

		public abstract Task<IPAddress> GetExternalIPAsync();

		public abstract Task<Mapping> GetSpecificMappingAsync(Protocol protocol, int port);

		protected void RegisterMapping(Mapping mapping)
		{
			_openedMapping.Remove(mapping);
			_openedMapping.Add(mapping);
		}

		protected void UnregisterMapping(Mapping mapping)
		{
			_openedMapping.RemoveWhere(x => x.Equals(mapping));
		}

        internal void ReleaseMapping(IEnumerable<Mapping> mappings)
		{
			var maparr = mappings.ToArray();
			var mapCount = maparr.Length;
			NatDiscoverer.TraceSource.LogInfo("{0} ports to close", mapCount);
			for (var i = 0; i < mapCount; i++)
			{
				var mapping = _openedMapping.ElementAt(i);
                try
				{
					DeletePortMapAsync(mapping).Wait();
					NatDiscoverer.TraceSource.LogInfo(mapping + " port successfully closed");
				}
				catch (Exception)
				{
					NatDiscoverer.TraceSource.LogError(mapping + " port couldn't be close");
				}
			}
		}

		internal void ReleaseAll()
		{
			ReleaseMapping(_openedMapping);
		}

		internal void ReleaseSessionMappings()
		{
			var mappings = from m in _openedMapping
						   where m.LifetimeType == MappingLifetime.Session
						   select m;
            ReleaseMapping(mappings);
		}

        internal async Task RenewMappings()
		{
			var mappings = _openedMapping.Where(x => x.ShoundRenew());
			foreach (var mapping in mappings.ToArray())
			{
				var m = mapping;
				await RenewMapping(m);
			}
		}

		private async Task RenewMapping(Mapping mapping)
		{
			var renewMapping = new Mapping(mapping);
			try
			{
				renewMapping.Expiration = DateTime.UtcNow.AddSeconds(mapping.Lifetime);
                NatDiscoverer.TraceSource.LogInfo("Renewing mapping {0}", renewMapping);
				await CreatePortMapAsync(renewMapping);
				NatDiscoverer.TraceSource.LogInfo("Next renew scheduled at: {0}", renewMapping.Expiration.ToLocalTime().TimeOfDay);
			}
			catch (Exception)
			{
				NatDiscoverer.TraceSource.LogWarn("Renew {0} failed", mapping);
			}
		}
    }
}