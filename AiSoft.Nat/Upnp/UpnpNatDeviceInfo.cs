using System;
using System.Net;
using AiSoft.Nat.Base;
using AiSoft.Nat.Utils;

namespace AiSoft.Nat.Upnp
{
	internal class UpnpNatDeviceInfo
	{
		public UpnpNatDeviceInfo(IPAddress localAddress, Uri locationUri, string serviceControlUrl, string serviceType)
		{
			LocalAddress = localAddress;
			ServiceType = serviceType;
			HostEndPoint = new IPEndPoint(IPAddress.Parse(locationUri.Host), locationUri.Port);

			if (Uri.IsWellFormedUriString(serviceControlUrl, UriKind.Absolute))
			{
				var u = new Uri(serviceControlUrl);
				var old = HostEndPoint;
				serviceControlUrl = u.PathAndQuery;

				NatDiscoverer.TraceSource.LogInfo("{0}: Absolute URI detected. Host address is now: {1}", old, HostEndPoint);
				NatDiscoverer.TraceSource.LogInfo("{0}: New control url: {1}", HostEndPoint, serviceControlUrl);
			}

			var builder = new UriBuilder("http", locationUri.Host, locationUri.Port);
			ServiceControlUri = new Uri(builder.Uri, serviceControlUrl); ;
		}

		public IPEndPoint HostEndPoint { get; private set; }

		public IPAddress LocalAddress { get; private set; }

		public string ServiceType { get; private set; }

		public Uri ServiceControlUri { get; private set; }
	}
}