using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
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

            while (true)
            {
                using (var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
                {
                    Log("Connecting to master...");

                    pipe.Connect();

                    Log("Connected");

                    Task.Run(() => ReadMessages(stdin, pipe));

                    ReadLoop(pipe, stdout);
                }

                Log("Restarting");
            }
        }

        private void ReadLoop(Stream inStream, Stream outStream)
        {
            var buffer = new byte[1024];
            int read;

            while ((read = inStream.Read(buffer)) != 0)
            {
                Log($"Read {read} bytes from master");

                outStream.Write(buffer, 0, read);
            }

            Log("Exited pipe read loop");
        }

        private void ReadMessages(Stream inStream, Stream outStream)
        {
            var buffer = new byte[1024];
            var sizeBuffer = new byte[4];
            int read, messageSize;

            var message = new MemoryStream();

            while (true)
            {
                if (inStream.Read(sizeBuffer) == 0)
                    return;

                messageSize = BitConverter.ToInt32(sizeBuffer);

                Log($"Message size is {messageSize} bytes");
                Log(BitConverter.ToString(sizeBuffer));

                while (message.Length < messageSize)
                {
                    if ((read = inStream.Read(buffer)) == 0)
                        return;

                    Log($"Read {read} bytes from browser");
                    Log(BitConverter.ToString(sizeBuffer));

                    message.Write(buffer, 0, read);
                }

                message.Position = 0;
                message.CopyTo(outStream);
                message.SetLength(0);
            }
        }
    }
}
