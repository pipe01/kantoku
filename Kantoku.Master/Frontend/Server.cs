using Kantoku.Master.Helpers;
using Serilog;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Kantoku.Master.Frontend
{
    public interface IServer : IHosted
    {
    }

    public class Server : IServer
    {
        private const ushort Port = 4545;

        private readonly ILogger Logger;
        private readonly WebSocketServer WebServer;

        public Server(ILogger logger)
        {
            this.Logger = logger.For<Server>();

            WebServer = new WebSocketServer(4545);
            WebServer.AddWebSocketService<Behaviour>("/ws");
        }

        public Task Start()
        {
            WebServer.Start();

            Logger.Debug("Listening on {Port}", Port);

            return Task.CompletedTask;
        }

        private class Behaviour : WebSocketBehavior
        {
            protected override void OnMessage(MessageEventArgs e)
            {
                Send(e.Data);
            }
        }
    }
}
