using Serilog;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Kantoku.Master.Helpers
{
    public sealed class MessageReader : IAsyncEnumerator<JsonDocument?>, IAsyncEnumerable<JsonDocument?>
    {
        private static ILogger Logger = Log.ForContext<MessageReader>();

        private readonly PipeReader Reader;
        private bool Completed;

        public JsonDocument? Current { get; private set; }

        public MessageReader(Stream stream)
        {
            this.Reader = PipeReader.Create(stream);
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (Completed)
                return false;

            var result = await Reader.ReadAsync();
            this.Completed = result.IsCompleted;

            if (result.IsCompleted && result.Buffer.Length == 0)
                return false;

            if (!TryReadLength(result.Buffer, out var dataLength))
                throw new InvalidOperationException($"Not enough data to read length");

            Reader.AdvanceTo(result.Buffer.GetPosition(sizeof(int)));

            while (true)
            {
                result = await Reader.ReadAsync();

                Logger.Verbose("Read {Length} bytes", result.Buffer.Length);

                if (result.Buffer.Length >= dataLength)
                {
                    var data = result.Buffer.Slice(0, dataLength);

                    Current = JsonDocument.Parse(data);

                    Reader.AdvanceTo(data.End);
                    break;
                }
                else
                {
                    Reader.AdvanceTo(result.Buffer.Start, result.Buffer.End);
                }
            }

            return true;

            static bool TryReadLength(in ReadOnlySequence<byte> seq, out int length)
            {
                var reader = new SequenceReader<byte>(seq);
                return reader.TryReadLittleEndian(out length);
            }
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public IAsyncEnumerator<JsonDocument> GetAsyncEnumerator(CancellationToken cancellationToken = default) => this;
    }
}
