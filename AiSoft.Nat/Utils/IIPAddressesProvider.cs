using System.Collections.Generic;
using System.Net;

namespace AiSoft.Nat.Utils
{
	internal interface IIPAddressesProvider
	{
		IEnumerable<IPAddress> DnsAddresses();

		IEnumerable<IPAddress> GatewayAddresses();

		IEnumerable<IPAddress> UnicastAddresses();
	}
}