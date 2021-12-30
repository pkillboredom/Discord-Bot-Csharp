using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot
{
    public class DiscordBot
    {
        private readonly IServiceProvider serviceProvider;

        public DiscordBot(IServiceProvider _serviceProvider)
        {
            serviceProvider = _serviceProvider;
        }

        public async Task MainAsync()
        {

            Console.WriteLine("Ready for takeoff...");
            var client = serviceProvider.GetRequiredService<DiscordSocketClient>();

            client.Log += Log;
            serviceProvider.GetRequiredService<CommandService>().Log += Log;

            // Get the bot token from the config.json
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            string token = config["token"];

            // Log in to Discord and start the bot.
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await serviceProvider.GetRequiredService<CommandHandlingService>().InitializeAsync();

            // Run the bot forever.
            await Task.Delay(-1);
        }
        private Task Log(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}
