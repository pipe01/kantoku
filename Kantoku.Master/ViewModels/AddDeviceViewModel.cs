using Kantoku.Master.Helpers;
using PropertyChanged;
using QRCoder;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kantoku.Master.ViewModels
{
    public class AddDeviceViewModel : INotifyPropertyChanged
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool UglyQR
        {
            get => Config.UseUglyQR;
            set
            {
                Config.UseUglyQR = value;

                PropertyChanged.Invoke(this, nameof(AddressCode));
            }
        }
        public ImageSource AddressCode => GenerateNetworkQR();

        private (bool Ugly, ImageSource Image)? CachedQR;

        private readonly INetwork Network;
        private readonly Config Config;

        public AddDeviceViewModel(INetwork network, Config config)
        {
            this.Network = network;
            this.Config = config;
        }

        private ImageSource GenerateNetworkQR()
        {
            if (CachedQR != null && CachedQR.Value.Ugly == UglyQR)
            {
                return CachedQR.Value.Image;
            }

            var addresses = Network.GetAllLocalIPv4().Select(o => o + ":" + Config.RemotePort);
            var payload = JsonSerializer.Serialize(addresses);

            var qr = GenerateQR(payload, !UglyQR);
            CachedQR = (UglyQR, qr);

            return qr;
        }

        private static ImageSource GenerateQR(string text, bool pretty = true)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCode = new QRCode(qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.M));
            using var qrCodeImage = qrCode.GetGraphic(20, pretty ? "#C2C2C2" : "#000000", pretty ? "#424242" : "#FFFFFF");
            var hbitmap = qrCodeImage.GetHbitmap();

            try
            {
                var image = Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                image.Freeze();

                return image;
            }
            finally
            {
                DeleteObject(hbitmap);
            }
        }
    }
}
