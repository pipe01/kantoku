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

namespace Kantoku.Master.ViewModels
{
    public class AddDeviceViewModel : INotifyPropertyChanged
    {
        public string[] Addresses { get; }

        public int SelectedAddressIndex { get; set; }
        public bool ControlsShown { get; set; }

        public string SelectedAddress => Addresses[SelectedAddressIndex];

        public bool HasLeft => SelectedAddressIndex > 0;
        public bool HasRight => SelectedAddressIndex < Addresses.Length - 1;

        public ImageSource AddressCode => GenerateQR(SelectedAddress + ":4545");

        public ICommand GoLeft { get; }
        public ICommand GoRight { get; }
        public ICommand ShowControls { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public AddDeviceViewModel()
        {
            this.Addresses = GetAllLocalIPv4();

            this.GoLeft = new ActionCommand(() => SelectedAddressIndex--);
            this.GoRight = new ActionCommand(() => SelectedAddressIndex++);
            this.ShowControls = new ActionCommand(() => ControlsShown = true);
        }

        private static ImageSource GenerateQR(string text)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(40);

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
