using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Kantoku.Master.Helpers
{
    public interface INetwork
    {
        string[] GetAllLocalIPv4();
        void BroadcastDiscovery();
    }

    public class Network : INetwork
    {
        private const int BroadcastPort = 54250;

        private readonly ILogger Logger;
        private readonly UdpClient Client;
        private readonly Random Random = new Random();

        public Network(ILogger<Network> logger)
        {
            this.Logger = logger;

            this.Client = new UdpClient
            {
                EnableBroadcast = true
            };
        }

        public string[] GetAllLocalIPv4()
        {
            return GetUnicastAddresses().Select(o => o.Address.ToString()).OrderBy(o => o).ToArray();
        }

        public void BroadcastDiscovery()
        {
            Logger.Debug("Broadcasting discovery");

            var data = new byte[10];
            Random.NextBytes(data);

            foreach (var item in GetUnicastAddresses())
            {
                var broadcastAddr = GetBroadcastAddress(item.Address, item.IPv4Mask);

                Logger.Verbose("Broadcasting to {Address}", broadcastAddr);

                Client.Send(data, data.Length, new IPEndPoint(broadcastAddr, BroadcastPort));
            }
        }

        private static IEnumerable<UnicastIPAddressInformation> GetUnicastAddresses()
        {
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip.Address))
                        {
                            yield return ip;
                        }
                    }
                }
            }
        }

        private static IPAddress GetBroadcastAddress(IPAddress address, IPAddress mask)
        {
            uint ipAddress = BitConverter.ToUInt32(address.GetAddressBytes(), 0);
            uint ipMaskV4 = BitConverter.ToUInt32(mask.GetAddressBytes(), 0);
            uint broadCastIpAddress = ipAddress | ~ipMaskV4;

            return new IPAddress(BitConverter.GetBytes(broadCastIpAddress));
        }
    }
}
