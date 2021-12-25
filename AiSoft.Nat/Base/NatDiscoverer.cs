using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AiSoft.Nat.Exceptions;
using AiSoft.Nat.Pmp;
using AiSoft.Nat.Upnp;
using AiSoft.Nat.Utils;

namespace AiSoft.Nat.Base
{
    public class NatDiscoverer
	{
        public readonly static TraceSource TraceSource = new TraceSource("AiSoft.Nat");

		private static readonly Dictionary<string, NatDevice> Devices = new Dictionary<string, NatDevice>();

		private static readonly Finalizer Finalizer = new Finalizer();

		internal static readonly Timer RenewTimer = new Timer(RenewMappings, null, 5000, 2000);

        public async Task<NatDevice> DiscoverDeviceAsync()
        {
            var cts = new CancellationTokenSource(3 * 1000);
            return await DiscoverDeviceAsync(PortMapper.Pmp | PortMapper.Upnp, cts);
        }

        public async Task<NatDevice> DiscoverDeviceAsync(PortMapper portMapper, CancellationTokenSource cancellationTokenSource)
		{
			Guard.IsTrue(portMapper.HasFlag(PortMapper.Upnp) || portMapper.HasFlag(PortMapper.Pmp), "portMapper");
			Guard.IsNotNull(cancellationTokenSource, "cancellationTokenSource");

			var devices = await DiscoverAsync(portMapper, true, cancellationTokenSource);
			var device = devices?.FirstOrDefault();
			if(device == null)
			{
				TraceSource.LogInfo("Device not found. Common reasons:");
				TraceSource.LogInfo("\t* No device is present or,");
				TraceSource.LogInfo("\t* Upnp is disabled in the router or");
				TraceSource.LogInfo("\t* Antivirus software is filtering SSDP (discovery protocol).");
				throw new NatDeviceNotFoundException();
			}
			return device;
		}

		public async Task<IEnumerable<NatDevice>> DiscoverDevicesAsync(PortMapper portMapper, CancellationTokenSource cancellationTokenSource)
		{
			Guard.IsTrue(portMapper.HasFlag(PortMapper.Upnp) || portMapper.HasFlag(PortMapper.Pmp), "portMapper");
			Guard.IsNotNull(cancellationTokenSource, "cancellationTokenSource");

			var devices = await DiscoverAsync(portMapper, false, cancellationTokenSource);
			return devices.ToArray();
		}

		private async Task<IEnumerable<NatDevice>> DiscoverAsync(PortMapper portMapper, bool onlyOne, CancellationTokenSource cts)
		{
			TraceSource.LogInfo("Start Discovery");
			var searcherTasks = new List<Task<IEnumerable<NatDevice>>>();
			if(portMapper.HasFlag(PortMapper.Upnp))
			{
				var upnpSearcher = new UpnpSearcher(new IPAddressesProvider());
				upnpSearcher.DeviceFound += (sender, args) => { if (onlyOne) cts.Cancel(); };
				searcherTasks.Add(upnpSearcher.Search(cts.Token));
			}
			if(portMapper.HasFlag(PortMapper.Pmp))
			{
				var pmpSearcher = new PmpSearcher(new IPAddressesProvider());
				pmpSearcher.DeviceFound += (sender, args) => { if (onlyOne) cts.Cancel(); };
				searcherTasks.Add(pmpSearcher.Search(cts.Token));
			}

            await Task.WhenAll(searcherTasks);
			TraceSource.LogInfo("Stop Discovery");
			
			var devices = searcherTasks.SelectMany(x => x.Result);
			foreach (var device in devices)
			{
				var key = device.ToString();
				NatDevice nat;
				if(Devices.TryGetValue(key, out nat))
				{
					nat.Touch();
				}
				else
				{
                    Devices.Add(key, device);
				}
			}
			return devices;
		}

		public static void ReleaseAll()
		{
			foreach (var device in Devices.Values)
			{
				device.ReleaseAll();
			}
		}

		internal static void ReleaseSessionMappings()
		{
			foreach (var device in Devices.Values)
			{
				device.ReleaseSessionMappings();
			}
		}

		private static void RenewMappings(object state)
		{
            Task.Factory.StartNew(async () =>
			{
				foreach (var device in Devices.Values)
				{
					await device.RenewMappings();
				}
			});
        }
	}
}