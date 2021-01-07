using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Kantoku.Master
{
    public class Satellite
    {
        public const string PipeName = "Kantoku";

        private readonly StreamWriter LogWriter;

#if DEBUG
        public Satellite()
        {
            this.LogWriter = new StreamWriter($"logs/satellite.txt")
            {
                AutoFlush = true
            };

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log(e.ExceptionObject.ToString());
        }
#endif

        public static bool IsAppSatellite(string[] args) => args.Length >= 1;

        [Conditional("DEBUG")]
        private void Log(string? message)
        {
            lock (LogWriter)
                LogWriter.WriteLine(message);
        }

        public void Run()
        {
            var stdin = Console.OpenStandardInput();
            var stdout = Console.OpenStandardOutput();

            using var master = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

            Log("Connecting to master...");

            master.Connect();

            Log("Connected");

            Task.Run(() => ReadLoop(stdin, master, "browser"));
            ReadLoop(master, stdout, "master");
        }

        private void ReadLoop(Stream inStream, Stream outStream, string from)
        {
            var buffer = new byte[1024];
            int read;

            while ((read = inStream.Read(buffer)) != 0)
            {
                Log($"Read {read} bytes from {from}");

                outStream.Write(buffer, 0, read);
            }

            Log($"Exited read loop from {from}");
        }
    }
}
