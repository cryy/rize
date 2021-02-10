using Discord.Audio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using System.Diagnostics;
using System.Net.Http;
using Discord;
using System.IO;
using YoutubeExplode.Videos.Streams;
using System.Collections.Immutable;
using Nerdbank.Streams;

namespace rize.Audio
{
    public class Player
    {
        public ITrack CurrentTrack { get; private set; }
        public TimeSpan CurrentPosition { get; private set; }
        public ConcurrentQueue<ITrack> Queue { get; private set; }

        IAudioClient _audio;
        ITextChannel _text;
        YoutubeClient _yt;
        PlayerOptions _options;


        AudioOutStream _pcm;

        CancellationTokenSource _streamCts;


        public Player(IAudioClient audioClient, ITextChannel c, YoutubeClient youtube, PlayerOptions options)
        {
            _audio = audioClient;
            _text = c;
            _yt = youtube;
            _options = options ?? new PlayerOptions();

            Queue = new ConcurrentQueue<ITrack>();
            CurrentPosition = TimeSpan.Zero;
        }

        public async Task<bool> QueueAsync(ITrack track)
        {
            if (Queue.IsEmpty && CurrentTrack == null)
            {
                await NextAsync(track);
                return false;
            }
            else
                Queue.Enqueue(track);

            return true;
        }

        public async Task NextAsync(ITrack track)
        {
            CurrentTrack = track;

            if (_pcm == null)
                _pcm = _audio.CreatePCMStream(AudioApplication.Music, packetLoss: _options.PacketLoss);

            string url = null;

            if (track is YoutubeTrack t)
            {
                var manifest = await _yt.Videos.Streams.GetManifestAsync(t.Id);
                url = manifest.GetAudioOnly().WithHighestBitrate().Url;
            }
            else
                url = track.Url;

            _streamCts = new CancellationTokenSource();

            _ = StreamAsync(_streamCts.Token, url).ContinueWith((t) =>
            {
                if (!Queue.IsEmpty && Queue.TryDequeue(out ITrack track))
                    _ = NextAsync(track);
            });
        }



        public Task StreamAsync(CancellationToken ct, string url)
           => Task.Run(async () =>
           {
               try
               {
                   using (var ffmpeg = SpawnFFMpeg(url))
                   using (var output = ffmpeg.StandardOutput.BaseStream)
                   {

                       try
                       {
                           await output.CopyToAsync(_pcm);
                       }
                       finally
                       {
                           await _pcm.FlushAsync();
                       }
                   }
               }
               catch (Exception e)
               {
                   Console.WriteLine(e);
               }
           }, ct);

        Process SpawnFFMpeg(string url)
        {
            var p = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-reconnect 1 -reconnect_streamed 1 -reconnect_delay_max 5 -hide_banner -i \"{url}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            p.ErrorDataReceived += (o, data) =>
            {
                var d = data.Data;
                if (d != null)
                {
                    var i = d.IndexOf("time=");
                    if (i != -1 && TimeSpan.TryParse(d.Substring(i + 5, 11), out TimeSpan pos))
                        CurrentPosition = pos;
                }
            };

            p.Start();
            p.BeginErrorReadLine();

            return p;
        }

        Process SpawnFFProbe(string url)
        {
            return null;
        }

    }
}
