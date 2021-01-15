using System.Windows.Input;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Kantoku.Master.Helpers;
using System.ComponentModel;
using QRCoder;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Interop;
using System;
using System.Windows;
using System.Text.Json;

using Color = System.Drawing.Color;
using System.Linq;

namespace Kantoku.Master.ViewModels
{
    public class AddDeviceViewModel : INotifyPropertyChanged
    {
        public ImageSource AddressCode { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public AddDeviceViewModel()
        {
            var addresses = GetAllLocalIPv4();

            AddressCode = GenerateQR(JsonSerializer.Serialize(addresses.Select(o => o + ":4545")));
        }

        private static ImageSource GenerateQR(string text)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);

            // TODO: Delete Hbitmap object
            return Imaging.CreateBitmapSourceFromHBitmap(qrCodeImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        // https://stackoverflow.com/a/24814027
        private static string[] GetAllLocalIPv4()
        {
            List<string> ipAddrList = new List<string>();

            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip.Address))
                        {
                            ipAddrList.Add(ip.Address.ToString());
                        }
                    }
                }
            }

            ipAddrList.Sort();
            return ipAddrList.ToArray();
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
