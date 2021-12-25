using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AiSoft.Nat.Base;
using AiSoft.Nat.EventArgs;
using AiSoft.Nat.Utils;

namespace AiSoft.Nat.Discovery
{
	internal abstract class Searcher
	{
		private readonly List<NatDevice> _devices = new List<NatDevice>(); 
		protected List<UdpClient> UdpClients;
		public EventHandler<DeviceEventArgs> DeviceFound; 
        internal DateTime NextSearch = DateTime.UtcNow;

		public async Task<IEnumerable<NatDevice>> Search(CancellationToken cancelationToken)
		{
			await Task.Factory.StartNew(_ =>
				{
					NatDiscoverer.TraceSource.LogInfo("Searching for: {0}", GetType().Name);
					while (!cancelationToken.IsCancellationRequested)
					{
						Discover(cancelationToken);
						Receive(cancelationToken);
					}
					CloseUdpClients();
				}, null, cancelationToken);
			return _devices;
		}

        private void Discover(CancellationToken cancelationToken)
		{
            if (DateTime.UtcNow < NextSearch)
            {
                return;
            }
            foreach (var socket in UdpClients)
			{
				try
				{
					Discover(socket, cancelationToken);
				}
				catch (Exception e)
				{
					NatDiscoverer.TraceSource.LogError("Error searching {0} - Details:", GetType().Name);
					NatDiscoverer.TraceSource.LogError(e.ToString());
				}
			}
		}

		private void Receive(CancellationToken cancelationToken)
		{
			foreach (var client in UdpClients.Where(x=>x.Available>0))
			{
                if (cancelationToken.IsCancellationRequested)
                {
                    return;
                }
                var localHost = ((IPEndPoint)client.Client.LocalEndPoint).Address;
				var receivedFrom = new IPEndPoint(IPAddress.None, 0);
				var buffer = client.Receive(ref receivedFrom);
				var device = AnalyseReceivedResponse(localHost, buffer, receivedFrom);
                if (device != null)
                {
                    RaiseDeviceFound(device);
                }
			}
		}

        protected abstract void Discover(UdpClient client, CancellationToken cancelationToken);

		public abstract NatDevice AnalyseReceivedResponse(IPAddress localAddress, byte[] response, IPEndPoint endpoint);

		public void CloseUdpClients()
		{
			foreach (var udpClient in UdpClients)
			{
				udpClient.Close();
			}
		}

		private void RaiseDeviceFound(NatDevice device)
		{
			_devices.Add(device);
			var handler = DeviceFound;
            if (handler != null)
            {
                handler(this, new DeviceEventArgs(device));
            }
		}
	}
}