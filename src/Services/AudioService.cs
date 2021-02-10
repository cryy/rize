using Discord;
using Discord.Commands;
using Discord.WebSocket;
using rize.Audio;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;

namespace rize.Services
{
    public class AudioService
    {
        ConcurrentDictionary<ulong, Player> _players;
        DiscordSocketClient _discordClient;

        public YoutubeClient _ytClient { get; private set; }

        public AudioService(DiscordSocketClient discord, YoutubeClient youtube)
        {
            _players = new ConcurrentDictionary<ulong, Player>();

            _discordClient = discord;
            _ytClient = youtube;
        }

        public async Task<IEnumerable<YoutubeTrack>> SearchYoutubeAsync(string query)
        {
            var videos = await _ytClient.Search.GetVideosAsync(query, 0, 1);

            return videos.Select(x => new YoutubeTrack(x));
        }

        public async ValueTask<Player> CreateOrCreatePlayerAsync(SocketCommandContext ctx, PlayerOptions options = null)
        {
            if (_players.TryGetValue(ctx.Guild.Id, out Player player)) return player;

            var guildUser = ctx.User as SocketGuildUser;

            await guildUser.VoiceChannel.DisconnectAsync();
            var audioClient = await guildUser.VoiceChannel.ConnectAsync();

            var guildPlayer = new Player(audioClient, ctx.Channel as ITextChannel, _ytClient, options);


            _players.TryAdd(ctx.Guild.Id, guildPlayer);

            return guildPlayer;
        }

        public (bool Success, Player Player) GetPlayer(ulong id)
        {
            var result = _players.TryGetValue(id, out Player p);
            return (result, p);
        }


    }
}
