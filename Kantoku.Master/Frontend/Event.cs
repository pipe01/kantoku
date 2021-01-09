using System.Text.Json.Serialization;

namespace Kantoku.Master.Frontend
{
    public enum EventKind
    {
        SessionStart,
        SessionEnd,
        SessionUpdate,

        Pause,
        Play,
        Stop,
        Previous,
        Next,
        SetPosition,
    }

    public class Event
    {
        [JsonPropertyName("kind")]
        public EventKind Kind { get; set; }

        [JsonPropertyName("data")]
        public object? Data { get; set; }
        
        [JsonPropertyName("session")]
        public string? Session { get; set; }

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
