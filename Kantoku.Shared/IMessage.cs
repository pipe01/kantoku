using MessagePack;

namespace Kantoku.Shared
{
    [Union(1, typeof(StartSessionMessage))]
    public interface IMessage
    {
        public const int MaxMessageSize = 4096;
        public const int ServerPort = 34871;
    }

    [MessagePackObject]
    public class StartSessionMessage : IMessage
    {

    }
}
