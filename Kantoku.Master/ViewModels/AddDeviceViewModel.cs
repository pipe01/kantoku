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

using System.Linq;

namespace Kantoku.Master.ViewModels
{
    public class AddDeviceViewModel : INotifyPropertyChanged
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public ImageSource AddressCode { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public AddDeviceViewModel()
        {
            var addresses = GetAllLocalIPv4();

            AddressCode = GenerateQR(JsonSerializer.Serialize(addresses.Select(o => o + ":4545")));
        }

        private static ImageSource GenerateQR(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCode = new QRCode(qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q));
            using var qrCodeImage = qrCode.GetGraphic(20);
            var hbitmap = qrCodeImage.GetHbitmap();

            var image = Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(hbitmap);

            return image;
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
