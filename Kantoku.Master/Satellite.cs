using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
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
        }
#endif

        [Conditional("DEBUG")]
        private void Log(string message)
        {
            LogWriter.WriteLine(message);
        }

        public void Run()
        {
            Log("Connecting to master...");

            using var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            pipe.Connect();

            Log("Connected");

            Task.Run(() => ReadLoop(pipe, Console.OpenStandardOutput()));
            Task.Run(() => ReadLoop(Console.OpenStandardInput(), pipe));
            Thread.Sleep(Timeout.Infinite);
        }

        private void ReadLoop(Stream inStream, Stream outStream)
        {
            var buffer = new byte[1024];
            int read;

            while ((read = inStream.Read(buffer)) != 0)
            {
                Log($"Read {read} bytes");

                outStream.Write(buffer, 0, read);
            }
        }
    }
}
