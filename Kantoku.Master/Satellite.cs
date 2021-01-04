using Kantoku.Shared;
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

        public void Run()
        {
            Debug.WriteLine("Connecting to master...");

            using var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            pipe.Connect();

            Debug.WriteLine("Connected");

            Task.Run(() => ReadLoop(pipe, Console.OpenStandardOutput()));
            Task.Run(() => ReadLoop(Console.OpenStandardInput(), pipe));

            Console.Error.WriteLine("Redirecting data");
            Thread.Sleep(Timeout.Infinite);
        }

        private static void ReadLoop(Stream inStream, Stream outStream)
        {
            var buffer = new byte[1024];
            int read;

            while ((read = inStream.Read(buffer)) != 0)
            {
                Console.Error.WriteLine(read);

                outStream.Write(buffer, 0, read);
            }
        }
    }
}
