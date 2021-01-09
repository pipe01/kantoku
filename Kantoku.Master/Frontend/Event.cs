using System.Text.Json.Serialization;

namespace Kantoku.Master.Frontend
{
    public enum EventKind
    {
        SessionStart,
        SessionEnd,
        SessionUpdate,
    }

    public class Event
    {
        [JsonPropertyName("kind")]
        public EventKind Kind { get; set; }

        [JsonPropertyName("data")]
        public object? Data { get; set; }

        public Event()
        {
        }

        public Event(EventKind kind, object? data)
        {
            this.Kind = kind;
            this.Data = data;
        }
    }
}
