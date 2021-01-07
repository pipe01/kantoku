using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Kantoku.Master.Helpers
{
    public class MessageWriter : IDisposable
    {
        private readonly Stream Stream;
        private readonly BlockingCollection<ReadOnlyMemory<byte>> PendingWrites = new BlockingCollection<ReadOnlyMemory<byte>>();

        public MessageWriter(Stream stream)
        {
            this.Stream = stream;

            new Thread(WriteLoop)
            {
                Name = "Message writer thread",
                IsBackground = true
            }.Start();
        }

        public void Dispose()
        {
            PendingWrites.Dispose();
        }

        public void Write(ReadOnlyMemory<byte> data)
        {
            PendingWrites.Add(BitConverter.GetBytes(data.Length));
            PendingWrites.Add(data);
        }

        public void Write(params object[] values)
        {
            var str = JsonSerializer.Serialize(values);
            Write(Encoding.UTF8.GetBytes(str));
        }

        private async void WriteLoop()
        {
            while (true)
            {
                var data = PendingWrites.Take();

                await Stream.WriteAsync(data);
                Stream.Flush();
            }
        }
    }
}
