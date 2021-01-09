using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.ApplicationModel.Core;
using Windows.Management.Deployment;

#nullable disable

namespace Kantoku.Master.Helpers.Fetchers
{
    // Taken and adapted from https://github.com/ModernFlyouts-Community/ModernFlyouts/blob/ec89660f9f489420ade51ce115a0305f310db322/ModernFlyouts.Core/Media/Source/SourceModernAppInfo.cs
    public static class ModernFetcher
    {
        public static async Task<(bool Success, string AppName, string IconPath)> FetchInfo(string appId)
        {
            var pm = new PackageManager();
            var packages = pm.FindPackagesForUser("");

            var (found, app, path, index) = await GetAppListEntry(appId, packages);
            if (!found)
                return (false, null, null);

            var appName = app.DisplayInfo.DisplayName;

            var logoPath = GetRefinedLogoPath(path, index);

            if (!File.Exists(logoPath))
                logoPath = null;

            return (true, appName, logoPath);
        }

        private static async Task<(bool Found, AppListEntry App, string Path, int Index)> GetAppListEntry(string appId, IEnumerable<Windows.ApplicationModel.Package> packages)
        {
            foreach (var package in packages)
            {
                var result = await package.GetAppListEntriesAsync();
                for (int i = 0; i < result.Count; i++)
                {
                    var app = result[i];

                    if (app.AppUserModelId == appId)
                    {
                        return (true, app, package.InstalledPath, i);
                    }
                }
            }

            return (false, null, null, 0);
        }

        private static string GetLogoPathFromAppPath(string appPath, int appIndex)
        {
            var factory = (IAppxFactory)new AppxFactory();
            string logo = string.Empty;

            string manifestPath = Path.Combine(appPath, "AppXManifest.xml");
            const int STGM_SHARE_DENY_NONE = 0x40;

            _ = SHCreateStreamOnFileEx(manifestPath, STGM_SHARE_DENY_NONE, 0, false, IntPtr.Zero, out IStream strm);
            if (strm != null)
            {
                var reader = factory.CreateManifestReader(strm);
                var apps = reader.GetApplications();
                int i = 0;

                while (apps.GetHasCurrent())
                {
                    var app = apps.GetCurrent();
                    if (appIndex == i)
                    {
                        app.GetStringValue("Square44x44Logo", out logo);
                        break;
                    }
                    else
                    {
                        i++;
                        apps.MoveNext();
                    }
                }
                Marshal.ReleaseComObject(strm);
            }

            Marshal.ReleaseComObject(factory);
            return logo;
        }

        private static string GetRefinedLogoPath(string appPath, int appIndex)
        {
            var resourceName = GetLogoPathFromAppPath(appPath, appIndex);
            const string targetSizeToken = ".targetsize-";
            const string scaleToken = ".scale-";
            var sizes = new List<int>();
            string name = Path.GetFileNameWithoutExtension(resourceName);
            string ext = Path.GetExtension(resourceName);

            string finalSizeToken;
            if (Directory.EnumerateFiles(Path.Combine(appPath, Path.GetDirectoryName(resourceName)), name + targetSizeToken + "*" + ext).Any())
            {
                finalSizeToken = targetSizeToken;
            }
            else
            {
                finalSizeToken = scaleToken;
            }

            foreach (var file in Directory.EnumerateFiles(Path.Combine(appPath, Path.GetDirectoryName(resourceName)), name + finalSizeToken + "*" + ext))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                int pos = fileName.IndexOf(finalSizeToken) + finalSizeToken.Length;
                string sizeText = fileName[pos..];
                if (int.TryParse(sizeText, out int size))
                {
                    sizes.Add(size);
                }
            }
            if (sizes.Count == 0)
                return null;

            sizes.Sort();
            return Path.Combine(appPath, Path.GetDirectoryName(resourceName), name + finalSizeToken + sizes.First() + ext);
        }

        #region Appx Things

        [Guid("5842a140-ff9f-4166-8f5c-62f5b7b0c781"), ComImport]
        private class AppxFactory
        {
        }

        [Guid("BEB94909-E451-438B-B5A7-D79E767B75D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAppxFactory
        {
            void _VtblGap0_2(); // skip 2 methods

            IAppxManifestReader CreateManifestReader(IStream inputStream);
        }

        [Guid("4E1BD148-55A0-4480-A3D1-15544710637C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAppxManifestReader
        {
            void _VtblGap0_1(); // skip 1 method

            IAppxManifestProperties GetProperties();

            void _VtblGap1_5(); // skip 5 methods

            IAppxManifestApplicationsEnumerator GetApplications();
        }

        [Guid("9EB8A55A-F04B-4D0D-808D-686185D4847A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAppxManifestApplicationsEnumerator
        {
            IAppxManifestApplication GetCurrent();

            bool GetHasCurrent();

            bool MoveNext();
        }

        [Guid("5DA89BF4-3773-46BE-B650-7E744863B7E8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAppxManifestApplication
        {
            [PreserveSig]
            int GetStringValue([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.LPWStr)] out string vaue);
        }

        [Guid("03FAF64D-F26F-4B2C-AAF7-8FE7789B8BCA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAppxManifestProperties
        {
            [PreserveSig]
            int GetBoolValue([MarshalAs(UnmanagedType.LPWStr)] string name, out bool value);

            [PreserveSig]
            int GetStringValue([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.LPWStr)] out string vaue);
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int SHCreateStreamOnFileEx(string fileName, int grfMode, int attributes, bool create, IntPtr reserved, out IStream stream);

        #endregion
    }
}
