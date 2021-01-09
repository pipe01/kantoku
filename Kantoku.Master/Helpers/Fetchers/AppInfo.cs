using System.Windows.Media.Imaging;

namespace Kantoku.Master.Helpers.Fetchers
{
    public class AppInfo
    {
        public string Name { get; }
        public BitmapSource Icon { get; }

        public AppInfo(string name, BitmapSource icon)
        {
            this.Name = name;
            this.Icon = icon;
        }
    }
}
