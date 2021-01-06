using System;

namespace Kantoku.Master.Media
{
    public readonly struct MediaInfo
    {
        public string Title { get; }
        public string Author { get; }
        public TimeSpan Duration { get; }

        public MediaInfo(string title, string author, TimeSpan duration)
        {
            this.Title = title;
            this.Author = author;
            this.Duration = duration;
        }
    }
}
