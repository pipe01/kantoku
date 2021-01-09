using Kantoku.Master.Frontend;
using Kantoku.Master.Helpers;
using Kantoku.Master.Helpers.Fetchers;
using Kantoku.Master.Media.Services;
using Kantoku.Master.ViewModels;
using LightInject;
using Serilog;
using Serilog.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
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

                Task.Run(async () =>
                {
                    foreach (var item in container.GetAllInstances<IHosted>())
                    {
                        await item.Start();
                    }
                });
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
            var logger = Log.Logger.For("Container");

            var opts = new ContainerOptions
            {
                EnablePropertyInjection = false,
                LogFactory = type => entry =>
                {
                    var level = entry.Level switch
                    {
                        LogLevel.Info => LogEventLevel.Verbose,
                        LogLevel.Warning => LogEventLevel.Warning,
                        _ => throw new ArgumentException("Invalid log level")
                    };

                    logger.Write(level, entry.Message);
                }
            };

            var container = new ServiceContainer(opts);

            container.RegisterInstance<IServiceFactory>(container);
            container.RegisterInstance<IServiceRegistry>(container);
            container.RegisterInstance<IServiceContainer>(container);

            container.Register(typeof(ILogger<>), typeof(Logger<>));

            container.RegisterInstance(Log.Logger);
            container.RegisterInstance(Config.Load("config.yaml"));
            container.Register<IServiceManager, ServiceManager>();
            container.Register<IAppInfoFetcher, AppInfoFetcher>();
            container.Register<IServer, Server>();

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
