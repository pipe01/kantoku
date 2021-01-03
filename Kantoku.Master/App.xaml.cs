using Kantoku.Master.Services;
using LightInject;
using Serilog;
using System;
using System.Windows;

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

        protected override async void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Debug()
                .WriteTo.File("logs/kantoku.txt",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
                .CreateLogger();

            var container = new ServiceContainer();
            container.RegisterInstance(Log.Logger);
            container.RegisterInstance(Config.Load("config.yaml"));
            container.Register<IServiceManager, ServiceManager>();

            Log.Information("Kantoku starting up");

            await container.GetInstance<IServiceManager>().Start();

            base.OnStartup(e);
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
