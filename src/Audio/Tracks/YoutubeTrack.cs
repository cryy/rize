using YoutubeExplode.Videos;

namespace rize.Audio
{
    public class YoutubeTrack : ITrack
    {
        public string Title { get; }
        public string Url { get; }
        public string Id { get; }


        public YoutubeTrack(Video youtubeVid)
        {
            Title = youtubeVid.Title;
            Id = youtubeVid.Id;
            Url = $"https://www.youtube.com/watch?v={Id}";
        }

    }
}
