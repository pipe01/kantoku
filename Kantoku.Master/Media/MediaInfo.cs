using System;
using System.ComponentModel;

namespace Kantoku.Master.Media
{
    public class MediaInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Title { get; }
        public string Author { get; }
        public TimeSpan Duration { get; set; }

        public MediaInfo(string title, string author, TimeSpan duration)
        {
            this.Title = title;
            this.Author = author;
            this.Duration = duration;
        }
    }
}
