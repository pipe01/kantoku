using Kantoku.Master.Helpers;
using Kantoku.Master.Helpers.Fetchers;
using Kantoku.Master.Media.Services;
using Kantoku.Master.ViewModels;
using LightInject;
using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TypeFinder;

namespace Kantoku.Master
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            RegistryHelper.RegisterNativeHost();

            if (args.Length > 0 && args[0] == "--register")
                return;

            InitLogging();

            Log.Information("===========================================");
            Log.Information("Kantoku starting up");
            Log.Debug("Registering services");

            var container = InitContainer();

            var app = new App(container);
            app.InitializeComponent();
            app.Startup += (_, _) =>
            {
                container.RegisterInstance(SynchronizationContext.Current);

                _ = container.GetInstance<DashboardViewModel>().Start();
            };
            app.Run();
        }

        private static void InitLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss.ffff} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/kantoku.txt",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
                .CreateLogger();
        }

        private static ServiceContainer InitContainer()
        {
            var container = new ServiceContainer();
            container.RegisterInstance(Log.Logger);
            container.RegisterInstance(Config.Load("config.yaml"));
            container.Register<IServiceManager, ServiceManager>();
            container.Register<IAppInfoFetcher, AppInfoFetcher>();

            container.RegisterSingleton<DashboardViewModel>();
            container.RegisterSingleton<MainWindow>();

            foreach (var type in FindTypes.InCurrentAssembly.ThatInherit<IService>())
            {
                Log.Verbose("Found type {Name}", type.Name);

                container.RegisterSingleton(typeof(IService), type, type.Name);
            }

            return container;
        }
    }
}
