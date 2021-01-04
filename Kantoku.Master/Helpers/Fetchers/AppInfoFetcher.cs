using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Kantoku.Master.Helpers.Fetchers
{
    public interface IAppInfoFetcher
    {
        Task<AppInfo> FetchInfo(string appId);
    }

    public sealed class AppInfoFetcher : IAppInfoFetcher
    {
        private readonly ILogger Logger;

        public AppInfoFetcher(ILogger logger)
        {
            this.Logger = logger.For<AppInfoFetcher>();
        }

        public Task<AppInfo> FetchInfo(string appId)
        {
            Logger.Debug("Fetching app info for {App}", appId);

            if (IsWin32(appId))
                return FetchWin32(appId);
            else
                return FetchModern(appId);
        }

        private static bool IsWin32(string appId) => Path.GetExtension(appId).Equals(".exe", StringComparison.OrdinalIgnoreCase);

        private Task<AppInfo> FetchWin32(string appId)
        {
            Logger.Debug("App is win32");

            Process? proc = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appId)).FirstOrDefault();

            if (proc == null)
                throw new FileNotFoundException($"No process found for {appId}");

            Logger.Verbose("Found process ID {ID}", proc.Id);

            var appName = proc.MainModule?.FileVersionInfo.ProductName ?? "Unknown";

            Logger.Verbose("App name is {Name}", appName);

            var iconEx = new IconExtractor(proc.MainModule?.FileName);
            var icon = iconEx.GetIcon(0);
            var iconBmp = icon.ToBitmap();
            var iconSource = Imaging.CreateBitmapSourceFromHBitmap(iconBmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            iconSource.Freeze();

            return Task.FromResult(new AppInfo(appName, iconSource));
        }

        private async Task<AppInfo> FetchModern(string appId)
        {
            throw new NotImplementedException();
        }
    }
}
