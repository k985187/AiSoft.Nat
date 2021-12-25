using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AiSoft.Nat.Base;
using AiSoft.Nat.Enums;
using AiSoft.Nat.Exceptions;
using AiSoft.Nat.Utils;

namespace AiSoft.Nat.Pmp
{
	internal sealed class PmpNatDevice : NatDevice
	{
		public override IPEndPoint HostEndPoint => _hostEndPoint;

        public override IPAddress LocalAddress => _localAddress;

        private readonly IPEndPoint _hostEndPoint;
		private readonly IPAddress _localAddress;
		private readonly IPAddress _publicAddress;

		internal PmpNatDevice(IPAddress hostEndPointAddress, IPAddress localAddress, IPAddress publicAddress)
		{
			_hostEndPoint = new IPEndPoint(hostEndPointAddress, PmpConstants.ServerPort);
			_localAddress = localAddress;
			_publicAddress = publicAddress;
		}

		public override async Task CreatePortMapAsync(Mapping mapping)
		{
			await InternalCreatePortMapAsync(mapping, true).TimeoutAfter(TimeSpan.FromSeconds(4));
			RegisterMapping(mapping);
		}

        public override async Task DeletePortMapAsync(Mapping mapping)
		{
			await InternalCreatePortMapAsync(mapping, false).TimeoutAfter(TimeSpan.FromSeconds(4));
			UnregisterMapping(mapping);
		}

		public override Task<IEnumerable<Mapping>> GetAllMappingsAsync()
		{
			throw new NotSupportedException();
		}

		public override Task<IPAddress> GetExternalIPAsync()
		{
            return Task.Run(() => _publicAddress).TimeoutAfter(TimeSpan.FromSeconds(4));
		}

		public override Task<Mapping> GetSpecificMappingAsync(Protocol protocol, int port)
		{
			throw new NotSupportedException("NAT-PMP does not specify a way to get a specific port map");
		}

		private async Task<Mapping> InternalCreatePortMapAsync(Mapping mapping, bool create)
		{
			var package = new List<byte>();
            package.Add(PmpConstants.Version);
			package.Add(mapping.Protocol == Protocol.Tcp ? PmpConstants.OperationCodeTcp : PmpConstants.OperationCodeUdp);
			package.Add(0); //reserved
			package.Add(0); //reserved
			package.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short) mapping.PrivatePort)));
			package.AddRange(BitConverter.GetBytes(create ? IPAddress.HostToNetworkOrder((short) mapping.PublicPort) : (short) 0));
			package.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(mapping.Lifetime)));
            try
			{
				var buffer = package.ToArray();
				var attempt = 0;
				var delay = PmpConstants.RetryDelay;
                using (var udpClient = new UdpClient())
				{
					CreatePortMapListen(udpClient, mapping);
                    while (attempt < PmpConstants.RetryAttempts)
					{
						await udpClient.SendAsync(buffer, buffer.Length, HostEndPoint);
                        attempt++;
						delay *= 2;
						Thread.Sleep(delay);
					}
				}
			}
			catch (Exception e)
			{
				var type = create ? "create" : "delete";
				var message = $"Failed to {type} portmap (protocol={mapping.Protocol}, private port={mapping.PrivatePort})";
				NatDiscoverer.TraceSource.LogError(message);
				var pmpException = e as MappingException;
				throw new MappingException(message, pmpException);
			}
            return mapping;
		}

        private void CreatePortMapListen(UdpClient udpClient, Mapping mapping)
		{
			var endPoint = HostEndPoint;
            while (true)
			{
				var data = udpClient.Receive(ref endPoint);
                if (data.Length < 16)
                {
                    continue;
                }
                if (data[0] != PmpConstants.Version)
                {
                    continue;
                }
                var opCode = (byte) (data[1] & 127);
                var protocol = Protocol.Tcp;
				if (opCode == PmpConstants.OperationCodeUdp)
                {
					protocol = Protocol.Udp;

                }
                var resultCode = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 2));
				var epoch = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 4));

				var privatePort = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 8));
				var publicPort = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 10));

				var lifetime = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 12));

				if (privatePort < 0 || publicPort < 0 || resultCode != PmpConstants.ResultCodeSuccess)
				{
					var errors = new[]
									 {
										 "Success",
										 "Unsupported Version",
										 "Not Authorized/Refused (e.g. box supports mapping, but user has turned feature off)",
										 "Network Failure (e.g. NAT box itself has not obtained a DHCP lease)",
										 "Out of resources (NAT box cannot create any more mappings at this time)",
										 "Unsupported opcode"
									 };
					throw new MappingException(resultCode, errors[resultCode]);
				}
                if (lifetime == 0) 
                {
                    return; //mapping was deleted
                }
                //mapping was created
                mapping.PublicPort = publicPort;
				mapping.Protocol = protocol;
				mapping.Expiration = DateTime.Now.AddSeconds(lifetime);
				return;
			}
		}

		public override string ToString()
		{
			return $"Local Address: {HostEndPoint.Address}\nPublic IP: {_publicAddress}\nLast Seen: {LastSeen}";
		}
	}
}