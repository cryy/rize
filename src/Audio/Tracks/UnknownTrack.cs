namespace rize.Audio
{
    public class UnknownTrack : ITrack
    {

        public UnknownTrack(string url)
        {
            Url = url;
        }
        public string Title => "unknown";

        public string Url { get; }
    }
}
