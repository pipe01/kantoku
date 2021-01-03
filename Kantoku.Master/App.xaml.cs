using Kantoku.Master.Services;
using LightInject;
using Serilog;
using System.Windows;

namespace Kantoku.Master
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
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
            container.RegisterSingleton(_ => Log.Logger);
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
    }
}
