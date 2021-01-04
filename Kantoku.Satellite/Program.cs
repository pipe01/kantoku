using Kantoku.Shared;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Kantoku.Satellite
{
    class Program
    {
        static void Main(string[] args)
        {
            Debug.WriteLine("Connecting to master...");

            using var pipe = new NamedPipeClientStream(".", Constants.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
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
