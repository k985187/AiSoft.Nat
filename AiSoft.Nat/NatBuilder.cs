using System;
using System.Threading;
using System.Threading.Tasks;
using AiSoft.Nat.Base;
using AiSoft.Nat.Enums;

namespace AiSoft.Nat
{
    /// <summary>
    /// Nat映射类
    /// </summary>
    public static class NatBuilder
    {
        /// <summary>
        /// 添加一个Nat映射
        /// </summary>
        /// <param name="privatePort"></param>
        /// <param name="publicPort"></param>
        /// <param name="des"></param>
        /// <param name="isTcp"></param>
        /// <param name="isUPnP"></param>
        /// <param name="timeOut"></param>
        public static void Create(int privatePort = 12346, int publicPort = 12346, string des = "AiSoft.TcpServer.Nat", bool isTcp = true, bool isUPnP = true, int timeOut = 10000)
        {
            Task.Run(async () =>
            {
                var discoverer = new NatDiscoverer();
                var cts = new CancellationTokenSource(timeOut);
                var device = await discoverer.DiscoverDeviceAsync(isUPnP ? PortMapper.Upnp : PortMapper.Pmp, cts);
                await device.CreatePortMapAsync(new Mapping(isTcp ? Protocol.Tcp : Protocol.Udp, privatePort, publicPort, des));
            }).Wait();
        }

        /// <summary>
        /// 删除一个Nat映射
        /// </summary>
        /// <param name="privatePort"></param>
        /// <param name="publicPort"></param>
        /// <param name="des"></param>
        /// <param name="isTcp"></param>
        /// <param name="isUPnP"></param>
        /// <param name="timeOut"></param>
        public static void Delete(int privatePort = 12346, int publicPort = 12346, string des = "AiSoft.TcpServer.Nat", bool isTcp = true, bool isUPnP = true, int timeOut = 10000)
        {
            Task.Run(async () =>
            {
                var discoverer = new NatDiscoverer();
                var cts = new CancellationTokenSource(timeOut);
                var device = await discoverer.DiscoverDeviceAsync(isUPnP ? PortMapper.Upnp : PortMapper.Pmp, cts);
                await device.DeletePortMapAsync(new Mapping(isTcp ? Protocol.Tcp : Protocol.Udp, privatePort, publicPort, des));
            }).Wait();
        }

        /// <summary>
        /// 删除一个Nat映射
        /// </summary>
        /// <param name="isTcp"></param>
        /// <param name="isUPnP"></param>
        /// <param name="timeOut"></param>
        public static void DeleteAll(bool isTcp = true, bool isUPnP = true, int timeOut = 10000)
        {
            Task.Run(async () =>
            {
                var discoverer = new NatDiscoverer();
                var cts = new CancellationTokenSource(timeOut);
                var device = await discoverer.DiscoverDeviceAsync(isUPnP ? PortMapper.Upnp : PortMapper.Pmp, cts);
                var mappings = await device.GetAllMappingsAsync();
                foreach (var mapping in mappings)
                {
                    await device.DeletePortMapAsync(mapping);
                }
            }).Wait();
        }
    }
}