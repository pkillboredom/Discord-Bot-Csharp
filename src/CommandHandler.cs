using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Discord;
using System.Linq;

namespace Discord_Bot
{
    public class CommandHandlingService
    {
        private readonly CommandService Commands;
        private readonly DiscordSocketClient Client;
        private readonly IServiceProvider Services;

        public CommandHandlingService(IServiceProvider services)
        {

            Commands = services.GetRequiredService<CommandService>();
            Client = services.GetRequiredService<DiscordSocketClient>();
            Services = services;

            Client.Ready += Client_Ready; // Set config on client ready.
            Client.MessageReceived += HandleCommand; // Trigger the command handling task when a message is received.
            Client.JoinedGuild += SendJoinMessage; // Send the join message when joining guild.
        }

        public async Task HandleCommand(SocketMessage rawMessage)
        {
            if (rawMessage.Author.IsBot) return; // Ignore command if it's triggered by a bot.
            if (!(rawMessage is SocketUserMessage message)) return; // Cast message to SocketUserMessage.
            var Context = new SocketCommandContext(Client, message); // Declare Context variable.

            int argPos = 0; // Prefix position.

            JObject config = Functions.GetConfig();
            string prefix = config["prefix"].Value<string>(); // Get the "prefix" value.

            if (message.HasStringPrefix(prefix, ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos)) // Check if message has the prefix or mentioned the bot.
            {
                var result = await Commands.ExecuteAsync(Context, argPos, Services); // Execute the command.

                if (!result.IsSuccess && result.Error.HasValue)          
                    await Context.Channel.SendMessageAsync($":x: {result.ErrorReason}");          
            }
        }

        private async Task SendJoinMessage(SocketGuild guild)
        {                    
            foreach (var channel in guild.TextChannels.OrderBy(x => x.Position)) // Loop through every channel.
            {
                JObject config = Functions.GetConfig();
                string joinMessage = config["join_message"]?.Value<string>(); // Get the join message value.

                if (joinMessage == null || joinMessage == string.Empty) return; // If the join message is empty, cancel the join message.

                var botPerms = channel.GetPermissionOverwrite(Client.CurrentUser).GetValueOrDefault();
                if (botPerms.SendMessages == PermValue.Deny) continue; // If the bot doesn't have permission to send the welcome message, go to the next channel in the loop.
                try
                {
                    await channel.SendMessageAsync(joinMessage);
                    return;
                }
                catch { continue; }
            }
        }

        private async Task Client_Ready()
        => await Functions.SetBotStatus(Client); // Set bot status from the Config.json file.

        public async Task InitializeAsync()
             => await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services); // Add modules to command service.
    }
}