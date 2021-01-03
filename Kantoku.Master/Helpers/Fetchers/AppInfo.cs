using System.Windows.Media;

namespace Kantoku.Master.Helpers.Fetchers
{
    public class AppInfo
    {
        public string Name { get; }
        public ImageSource Icon { get; }

        public AppInfo(string name, ImageSource icon)
        {
            this.Name = name;
            this.Icon = icon;
        }
    }
}
