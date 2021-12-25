using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace AiSoft.Nat.Upnp.Messages
{
	internal static class DiscoverDeviceMessage
	{
        public static string Encode(string serviceType, IPAddress address)
		{
			var fmtAddress = string.Format(address.AddressFamily == AddressFamily.InterNetwork ? "{0}" : "[{0}]", address);

			var s = "M-SEARCH * HTTP/1.1\r\n"
						+ "HOST: " + fmtAddress + ":1900\r\n"
						+ "MAN: \"ssdp:discover\"\r\n"
						+ "MX: 3\r\n"
						+ "ST: urn:schemas-upnp-org:service:{0}\r\n\r\n";
			//+ "ST:upnp:rootdevice\r\n\r\n";

			return string.Format(CultureInfo.InvariantCulture, s, serviceType);
		}
	}
}