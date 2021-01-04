using Kantoku.Shared;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace Kantoku.Satellite
{
    class Program
    {
        static void Main(string[] args)
        {
            Debug.WriteLine("Connecting to master...");

            var pipe = new NamedPipeClientStream(".", Constants.PipeName, PipeDirection.InOut);
            pipe.Connect();

            Debug.WriteLine("Connected");

            pipe.Write(Encoding.UTF8.GetBytes("hello"));

            System.Console.WriteLine("Done");
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
