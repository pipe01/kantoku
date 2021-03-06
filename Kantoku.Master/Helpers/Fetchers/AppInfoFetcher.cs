﻿using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kantoku.Master.Helpers.Fetchers
{
    public interface IAppInfoFetcher
    {
        Task<AppInfo> FetchInfo(string appId);
    }

    public sealed class AppInfoFetcher : IAppInfoFetcher
    {
        private static readonly BitmapSource EmptyImage = BitmapSource.Create(2, 2, 96, 96, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { Colors.Transparent }), new byte[] { 0, 0, 0, 0 }, 1);

        private static readonly string CustomInfoFolder = Path.GetFullPath("custom");

        private readonly ILogger Logger;

        public AppInfoFetcher(ILogger logger)
        {
            this.Logger = logger.For<AppInfoFetcher>();
        }

        public async Task<AppInfo> FetchInfo(string appId)
        {
            Logger.Debug("Fetching app info for {App}", appId);

            var (customName, customIcon) = FetchFromCustom(appId);

            var fetched = IsWin32(appId) ? FetchWin32(appId) : await FetchModern(appId);

            return new AppInfo(customName ?? fetched.Name, customIcon ?? fetched.Icon);
        }

        private static bool IsWin32(string appId) => Path.GetExtension(appId).Equals(".exe", StringComparison.OrdinalIgnoreCase);

        private (string? AppName, BitmapSource? Icon) FetchFromCustom(string appId)
        {
            string? appName = null;
            BitmapSource? icon = null;

            if (!Directory.Exists(CustomInfoFolder))
                goto exit;

            var appNameFile = new FileInfo(Path.Combine(CustomInfoFolder, appId + ".txt"));
            if (appNameFile.Exists)
                appName = File.ReadAllText(appNameFile.FullName);

            var iconFile = new FileInfo(Path.Combine(CustomInfoFolder, appId + ".png"));
            if (iconFile.Exists)
            {
                icon = new BitmapImage(new Uri(iconFile.FullName));
                icon.Freeze();
            }

            exit:
            return (appName, icon);
        }

        private AppInfo FetchWin32(string appId)
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

            return new AppInfo(appName, iconSource);
        }

        private async Task<AppInfo> FetchModern(string appId)
        {
            Logger.Debug("App is modern");

            var (success, appName, iconPath) = await ModernFetcher.FetchInfo(appId);

            if (!success)
            {
                Logger.Verbose("Couldn't find info for app");

                return new AppInfo(appId, EmptyImage);
            }
            else
            {
                Logger.Verbose("Found info for app");

                var image = new BitmapImage(new Uri(iconPath));
                image.Freeze();

                return new AppInfo(appName, image);
            }
        }
    }
}
