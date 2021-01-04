using Kantoku.Master.Helpers.Fetchers;
using Kantoku.Master.Media.Services;
using Kantoku.Master.ViewModels;
using LightInject;
using Serilog;
using System;
using System.Windows;
using System.Windows.Threading;
using TypeFinder;

namespace Kantoku.Master
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (Satellite.IsAppSatellite(e.Args))
            {
                new Satellite().Run();
                Shutdown();
                return;
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/kantoku.txt",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
                .CreateLogger();

            Log.Information("===========================================");
            Log.Debug("Registering services");

            var container = new ServiceContainer();
            container.RegisterInstance(Log.Logger);
            container.RegisterInstance(Config.Load("config.yaml"));
            container.Register<IServiceManager, ServiceManager>();
            container.Register<IAppInfoFetcher, AppInfoFetcher>();

            container.RegisterInstance(Dispatcher);
            container.RegisterSingleton<DashboardViewModel>();
            container.RegisterSingleton<MainWindow>();

            foreach (var type in FindTypes.InCurrentAssembly.ThatInherit<IService>())
            {
                Log.Verbose("Found type {Name}", type.Name);

                container.RegisterSingleton(typeof(IService), type, type.Name);
            }

            Log.Information("Kantoku starting up");

            MainWindow = container.GetInstance<MainWindow>();
            MainWindow.Closed += delegate { Shutdown(); };
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Kantoku gracefully exiting");

            base.OnExit(e);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.ExceptionObject as Exception, "Unhandled exception");
        }
    }
}
