using Discord.Commands;
using rize.Audio;
using rize.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using YoutubeExplode.Videos;
using System.Text.RegularExpressions;

namespace rize.Commands
{
    public class TrackTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var audio = services.GetService<AudioService>();

            ITrack track;
            if (!Uri.IsWellFormedUriString(input, UriKind.Absolute))
            {
                var tracks = await audio.SearchYoutubeAsync(input);

                if (tracks.Count() == 0)
                    return TypeReaderResult.FromError(CommandError.ParseFailed, "No videos found.");

                track = tracks.FirstOrDefault();
            }
            else
            {
                if (Regex.IsMatch(input, @"(http|https)://(youtube.com|youtu.be)/", RegexOptions.IgnoreCase | RegexOptions.Compiled))
                {
                    var videoId = VideoId.TryParse(input).Value;
                    if(videoId == null)
                        return TypeReaderResult.FromError(CommandError.ParseFailed, "Could not parse YouTube URL.");

                    var vid = await audio._ytClient.Videos.GetAsync(videoId);
                    track = new YoutubeTrack(vid);
                }
                else
                    track = new UnknownTrack(input);
            }

            return TypeReaderResult.FromSuccess(track);
        }
    }
}
