using Serilog;
using Serilog.Events;

namespace Kantoku.Master.Helpers
{
    public interface ILogger<T> : ILogger { }

    public class Logger<T> : ILogger<T>
    {
        private readonly ILogger Inner;

        public Logger(ILogger logger)
        {
            this.Inner = logger.For<T>();
        }

        public void Write(LogEvent logEvent) => Inner.Write(logEvent);
    }
}
