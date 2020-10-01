using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Discord_Bot
{
    public class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            using var services = ConfigureServices();

            Console.WriteLine("Ready for takeoff...");
            var Client = services.GetRequiredService<DiscordSocketClient>();

            Client.Log += LogAsync;
            services.GetRequiredService<CommandService>().Log += LogAsync;

            JObject config = Functions.GetConfig();
            string Token = config["token"].Value<string>(); // Get the bot token from the Config.json file.

            await Client.LoginAsync(TokenType.Bot, Token); // Log in the bot to Discord.
            await Client.StartAsync(); // Start the bot.

            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(); // Initialize the command handling service.
            await Task.Delay(-1); // Run the bot forever.
        }

        public ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                { 
                    MessageCacheSize = 250,
                    LogLevel = LogSeverity.Info
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                { 
                    LogLevel = LogSeverity.Info,
                    DefaultRunMode = RunMode.Async,
                    CaseSensitiveCommands = false 
                }))
                .AddSingleton<CommandHandlingService>()
                .BuildServiceProvider();
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}