using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using rize.Audio;
using rize.Commands;
using rize.Services;
using System;
using System.Reflection;
using System.Threading.Tasks;
using YoutubeExplode;

namespace rize
{
    class Program
    {

        string _prefix;

        DiscordSocketClient _client;
        CommandService _commands;

        ServiceProvider _services;


        static void Main(string[] args)
            => new Program().MainAsync(args).GetAwaiter().GetResult();

        async Task MainAsync(string[] args)
        {
            _prefix = "rize ";

            _client = new DiscordSocketClient();
            _commands = new CommandService(new CommandServiceConfig {
                DefaultRunMode = RunMode.Async
            });

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton<YoutubeClient>()
                .AddSingleton<AudioService>()
                .BuildServiceProvider();

            _client.Log += Log;
            _client.MessageReceived += HandleCommandAsync;

            _commands.AddTypeReader(typeof(ITrack), new TrackTypeReader());
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // You can pass a token through the command line arguments or use an environmental variable.
            // If you don't want to use either of these options, remove the if else and directly assign the token.
            
            // string token = "token";

            string token;

            if (args.Length > 0)
                token = args[0];
            else
                token = Environment.GetEnvironmentVariable("bot_token");

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();


            await Task.Delay(-1);
        }


        Task HandleCommandAsync(SocketMessage _m)
        {
            // Return out of the handler if:
            //   the message is not a user message OR
            //   we aren't in a guild OR
            //   the author is a bot
            if (_m is not SocketUserMessage m 
                || m.Channel is IDMChannel 
                || m.Author.IsBot) return Task.CompletedTask;


            // Return out of the handler if there is no proper prefix
            int pos = 0;
            if (!(m.HasStringPrefix(_prefix, ref pos) || 
                m.HasMentionPrefix(_client.CurrentUser, ref pos))) return Task.CompletedTask;

            var ctx = new SocketCommandContext(_client, m);

            // Don't block while executing commands
            _ = _commands.ExecuteAsync(ctx, pos, _services);

            return Task.CompletedTask;
        }

        Task Log(LogMessage lm)
        {
            Console.WriteLine(lm);
            return Task.CompletedTask;
        }
    }
}
