using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Kantoku.Master.Helpers
{
    public static class Base64ImageEncoder
    {
        public static string Encode(BitmapSource image)
        {
            using var mem = new MemoryStream();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(mem);

            return Convert.ToBase64String(mem.GetBuffer()[..(int)mem.Length]);
        }
    }
}
