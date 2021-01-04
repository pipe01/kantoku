using MessagePack;

namespace Kantoku.Shared
{
    [MessagePackObject]
    public class Message
    {
        public const int MaxMessageSize = 4096;
    }
}
