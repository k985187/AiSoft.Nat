using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AiSoft.Nat.Base;
using AiSoft.Nat.Discovery;
using AiSoft.Nat.Utils;

namespace AiSoft.Nat.Pmp
{
	internal class PmpSearcher : Searcher
	{
		private readonly IIPAddressesProvider _ipprovider;
		private Dictionary<UdpClient, IEnumerable<IPEndPoint>> _gatewayLists;
		private int _timeout;

		internal PmpSearcher(IIPAddressesProvider ipprovider)
		{
			_ipprovider = ipprovider;
			_timeout = 250;
			CreateSocketsAndAddGateways();
		}

		private void CreateSocketsAndAddGateways()
		{
			UdpClients = new List<UdpClient>();
			_gatewayLists = new Dictionary<UdpClient, IEnumerable<IPEndPoint>>();
            try
			{
				var gatewayList = _ipprovider.GatewayAddresses().Select(ip => new IPEndPoint(ip, PmpConstants.ServerPort)).ToList();
                if (!gatewayList.Any())
				{
					gatewayList.AddRange(_ipprovider.DnsAddresses().Select(ip => new IPEndPoint(ip, PmpConstants.ServerPort)));
				}
                if (!gatewayList.Any())
                {
                    return;
                }
                foreach (var address in _ipprovider.UnicastAddresses())
				{
					UdpClient client;
                    try
					{
						client = new UdpClient(new IPEndPoint(address, 0));
					}
					catch (SocketException)
					{
						continue;
					}
                    _gatewayLists.Add(client, gatewayList);
					UdpClients.Add(client);
				}
			}
			catch (Exception e)
			{
				NatDiscoverer.TraceSource.LogError("There was a problem finding gateways: " + e);
            }
		}

		protected override void Discover(UdpClient client, CancellationToken cancelationToken)
		{
            NextSearch = DateTime.UtcNow.AddMilliseconds(_timeout);
			_timeout *= 2;
            if (_timeout >= 3000)
			{
				_timeout = 250;
				NextSearch = DateTime.UtcNow.AddSeconds(10);
				return;
			}
            var buffer = new[] {PmpConstants.Version, PmpConstants.OperationExternalAddressRequest};
			foreach (var gatewayEndpoint in _gatewayLists[client])
			{
                if (cancelationToken.IsCancellationRequested)
                {
                    return;
                }
                client.Send(buffer, buffer.Length, gatewayEndpoint);
			}
		}

		private bool IsSearchAddress(IPAddress address)
		{
			return _gatewayLists.Values.SelectMany(x => x).Any(x => x.Address.Equals(address));
		}

		public override NatDevice AnalyseReceivedResponse(IPAddress localAddress, byte[] response, IPEndPoint endpoint)
		{
            if (!IsSearchAddress(endpoint.Address) || response.Length != 12 || response[0] != PmpConstants.Version || response[1] != PmpConstants.ServerNoop)
            {
                return null;
            }
            int errorcode = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(response, 2));
            if (errorcode != 0)
            {
                NatDiscoverer.TraceSource.LogError("Non zero error: {0}", errorcode);
            }
            var publicIp = new IPAddress(new[] {response[8], response[9], response[10], response[11]});
			//NextSearch = DateTime.Now.AddMinutes(5);
            _timeout = 250;
			return new PmpNatDevice(localAddress, endpoint.Address, publicIp);
		}
	}
}