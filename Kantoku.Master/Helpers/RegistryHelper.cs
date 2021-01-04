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

            if (Registry.GetValue(currentUserKey, null, null) != null)
                return;

            Log.Debug("Registering native host in registry");

            string binary = Process.GetCurrentProcess().MainModule?.FileName ?? throw new Exception("Couldn't determine main module");

            if (!IsElevated)
            {
                Log.Information("Relaunching process as administrator");

                LaunchAsAdministrator(binary, Environment.GetCommandLineArgs()[1..]);
                Application.Current.Shutdown();
                return;
            }

            string manifestPath = Path.GetFullPath("manifest.json");
            string manifestContents = @$"
{{
  ""name"": ""kantoku"",
  ""description"": ""Kantoku satellite"",
  ""path"": ""{binary.Replace("\\", "\\\\")}"",
  ""type"": ""stdio"",
  ""allowed_extensions"": [ ""kantoku@pipe01.net"" ]
}}
";

            File.WriteAllText(manifestPath, manifestContents);

            Registry.SetValue(currentUserKey, null, manifestPath);
            Registry.SetValue(localMachineKey, null, manifestPath);
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
