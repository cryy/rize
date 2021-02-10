using Discord;
using Discord.Commands;
using Discord.WebSocket;
using rize.Audio;
using rize.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode.Videos;

namespace rize.Commands
{
    public class MusicModule : ModuleBase<SocketCommandContext>
    {
        AudioService _audio;

        public MusicModule(AudioService audio)
        {
            _audio = audio;
        }

        [Command("np")]
        public async Task NowPlayingAsync()
        {
            var p = _audio.GetPlayer(Context.Guild.Id);
            if (!p.Success) return;

            await ReplyAsync(p.Player.CurrentTrack.Title + "\n" + p.Player.CurrentPosition);
        }

        [Command("play")]
        public async Task PlayAsync([Remainder] ITrack track)
        {
            var player = await _audio.CreateOrCreatePlayerAsync(Context);

            var isQueued = await player.QueueAsync(track);
            await ReplyAsync(isQueued ? $"Added to queue: {track.Title}" : $"Now playing: {track.Title}");
        }
    }
}
