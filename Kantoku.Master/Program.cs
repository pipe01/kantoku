using System;

namespace Kantoku.Master
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (Satellite.IsAppSatellite(args))
            {
                new Satellite().Run();
            }
            else
            {
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }
    }
}
