using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Kantoku.Master
{
    public class Satellite
    {
        public static bool IsAppSatellite(string[] args) => args.Length >= 1;

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

            Stream? pipeStream = null;

            _ = Task.Run(() => ReadLoop(stdin, ref pipeStream));

            while (true)
            {
                using var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

                Log("Connecting to master...");

                pipe.Connect();

                Log("Connected");

                pipeStream = pipe;
                ReadLoop(pipe, ref stdout);

                Log("Restarting");
                pipeStream = null;
            }
        }

        private void ReadLoop(Stream inStream, ref Stream? outStream)
        {
            var buffer = new byte[1024];
            int read;

            while ((read = inStream.Read(buffer)) != 0)
            {
                Log($"Read {read} bytes");

                if (outStream == null)
                    continue;

                outStream.Write(buffer, 0, read);
                outStream.Flush();
            }

            Log("Exited sync read loop");
        }
    }
}
