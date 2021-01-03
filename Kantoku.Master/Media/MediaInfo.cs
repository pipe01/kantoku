namespace Kantoku.Master.Media
{
    public readonly struct MediaInfo
    {
        public string Title { get; }
        public string Author { get; }

        public MediaInfo(string title, string author)
        {
            this.Title = title;
            this.Author = author;
        }
    }
}
