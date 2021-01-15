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
        private readonly ServiceContainer? Container;

        public App()
        {
        }
        public App(ServiceContainer container)
        {
            this.Container = container;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (Container == null)
                return;

            FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
            {
                DefaultValue = FindResource(typeof(Window))
            });

            MainWindow = Container.GetInstance<MainWindow>();
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
