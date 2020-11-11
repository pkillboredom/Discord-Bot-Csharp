using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Discord;
using System.Linq;
using Newtonsoft.Json;

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

            Client.Ready += ClientReadyAsync;
            Client.MessageReceived += HandleCommandAsync;
            Client.JoinedGuild += SendJoinMessageAsync;
        }

        public async Task HandleCommandAsync(SocketMessage rawMessage)
        {
            // Ignore command if it's triggered by a bot.
            if (rawMessage.Author.IsBot) return;
            if (!(rawMessage is SocketUserMessage message)) return;

            var context = new SocketCommandContext(Client, message);

            int argPos = 0; // Prefix position.

            JObject config = Functions.GetConfig();
            string[] prefixes = JsonConvert.DeserializeObject<string[]>(config["prefixes"].ToString());

            // Check if message has the prefix or mentioned the bot.
            if (prefixes.Any(x => message.HasStringPrefix(x, ref argPos)) || message.HasMentionPrefix(Client.CurrentUser, ref argPos))
            {
                // Execute the command.
                var result = await Commands.ExecuteAsync(context, argPos, Services);

                if (!result.IsSuccess && result.Error.HasValue)          
                    await context.Channel.SendMessageAsync($":x: {result.ErrorReason}");          
            }
        }

        private async Task SendJoinMessageAsync(SocketGuild guild)
        {                    
            foreach (var channel in guild.TextChannels.OrderBy(x => x.Position))
            {
                JObject config = Functions.GetConfig();
                string joinMessage = config["join_message"]?.Value<string>();

                if (joinMessage == null || joinMessage == string.Empty) return;

                var botPerms = channel.GetPermissionOverwrite(Client.CurrentUser).GetValueOrDefault();
                if (botPerms.SendMessages == PermValue.Deny) continue;
                try
                {
                    await channel.SendMessageAsync(joinMessage);
                    return;
                }
                catch { continue; }
            }
        }

        private async Task ClientReadyAsync()
            => await Functions.SetBotStatusAsync(Client);

        public async Task InitializeAsync()
            => await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);
    }
}