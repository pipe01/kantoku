using Microsoft.Win32;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows;

namespace Kantoku.Master.Helpers
{
    public static class RegistryHelper
    {
        public static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public static void RegisterNativeHost()
        {
            const string currentUserKey = @"HKEY_CURRENT_USER\Software\Mozilla\NativeMessagingHosts\kantoku";
            const string localMachineKey = @"HKEY_LOCAL_MACHINE\Software\Mozilla\NativeMessagingHosts\kantoku";

            string manifestPath = Path.GetFullPath("manifest.json");

            WriteManifest(manifestPath);

            if (Registry.GetValue(currentUserKey, null, null) != null &&
                Registry.GetValue(localMachineKey, null, null) != null)
            {
                return;
            }

            Log.Debug("Registering native host in registry");

            if (!IsElevated)
            {
                Log.Information("Relaunching process as administrator");

                string binary = Process.GetCurrentProcess().MainModule?.FileName ?? throw new Exception("Couldn't determine main module");

                LaunchAsAdministrator(binary, Environment.GetCommandLineArgs()[1..]);
                Application.Current.Shutdown();
                return;
            }

            Registry.SetValue(currentUserKey, null, manifestPath);
            Registry.SetValue(localMachineKey, null, manifestPath);
        }

        private static void WriteManifest(string path)
        {
            string satelliteBin = Path.GetFullPath("Kantoku.Satellite.exe");

            if (!File.Exists(satelliteBin))
                throw new FileNotFoundException("The satellite binary could not be found");

            string manifestContents = @$"
{{
  ""name"": ""kantoku"",
  ""description"": ""Kantoku satellite"",
  ""path"": ""{satelliteBin.Replace("\\", "\\\\")}"",
  ""type"": ""stdio"",
  ""allowed_extensions"": [ ""kantoku@pipe01.net"" ]
}}
";

            File.WriteAllText(path, manifestContents);
        }

        private static void LaunchAsAdministrator(string path, string[] args)
        {
            var proc = new Process
            {
                StartInfo = new(path, string.Join('"', args))
                {
                    Verb = "runas",
                    UseShellExecute = true
                }
            };

            proc.Start();
        }
    }
}
