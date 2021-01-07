using Kantoku.Master.Helpers;
using Kantoku.Master.Helpers.Fetchers;
using Kantoku.Master.Media.Services;
using Kantoku.Master.ViewModels;
using LightInject;
using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using TypeFinder;

namespace Kantoku.Master
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            RegistryHelper.RegisterNativeHost();

            if (e.Args.Length > 0 && e.Args[0] == "--register")
            {
                Environment.Exit(0);
                return;
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/kantoku.txt",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
                .CreateLogger();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            Log.Information("===========================================");
            Log.Debug("Registering services");

            var container = new ServiceContainer();
            container.RegisterInstance(Log.Logger);
            container.RegisterInstance(Config.Load("config.yaml"));
            container.Register<IServiceManager, ServiceManager>();
            container.Register<IAppInfoFetcher, AppInfoFetcher>();

            container.RegisterInstance(Dispatcher);
            container.RegisterInstance(SynchronizationContext.Current);
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

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "Unhandled dispatcher exception");
        }
    }
}
