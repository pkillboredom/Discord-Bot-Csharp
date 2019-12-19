using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using Newtonsoft.Json;
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
            if (!(rawMessage is SocketUserMessage Message)) return; // Cast message to SocketUserMessage.
            var Context = new SocketCommandContext(Client, Message); // Declare Context variable.

            int ArgPos = 0; // Prefix position.

            using (StreamReader configjson = new StreamReader(Directory.GetCurrentDirectory().Replace(@"\bin\Debug", "").Replace(@"\bin\Release", "") + @"\src\Config.json")) // Get the config file. Note: this gets the current directory and removes the '\bin\Debug' or '\bin\Release' part and replaces it with 'Config.json'. Your directory may differ.
            {
                var readJSON = configjson.ReadToEnd(); // Read the Config.json file.
                var config = (JObject)JsonConvert.DeserializeObject(readJSON); // Deserialize the JSON.
                string prefix = config["prefix"].Value<string>(); // Get the "prefix" value.

                if (Message.HasStringPrefix(prefix, ref ArgPos) || Message.HasMentionPrefix(Client.CurrentUser, ref ArgPos)) // Check if message has the prefix or mentioned the bot.
                {
                    var Result = await Commands.ExecuteAsync(Context, ArgPos, Services); // Execute the command.
                    if (!Result.IsSuccess && Result.Error.HasValue) // If the command failed to execute...
                    {
                        await Context.Channel.SendMessageAsync($":x: {Result.ErrorReason}"); // Send message with the command error.
                    }
                }
            }
        }

        private async Task SendJoinMessage(SocketGuild guild)
        {                    
            foreach (var channel in guild.TextChannels.OrderBy(x => x.Position)) // Loop through every channel.
            {
                using (StreamReader configjson = new StreamReader(Directory.GetCurrentDirectory().Replace(@"\bin\Debug", "").Replace(@"\bin\Release", "") + @"\src\Config.json")) // Get the config file. Note: this gets the current directory and removes the '\bin\Debug' part and replaces it with 'Config.json'. Your directory may differ.
                {
                    var readJSON = configjson.ReadToEnd(); // Read the Config.json file.
                    var config = (JObject)JsonConvert.DeserializeObject(readJSON); // Deserialize the JSON.
                    string joinmessage = config["join_message"].Value<string>(); // Get the join message value.
                    if (joinmessage == "") return; // If the join message is empty, cancel the join message.

                    var bot = Client.CurrentUser as IUser; // Cast the bot to an IUser.
                    var botperms = channel.GetPermissionOverwrite(bot).GetValueOrDefault(); // Get the bot's permission in this channel.
                    if (botperms.SendMessages == PermValue.Deny) continue; // If the bot doesn't have permission to send the welcome message, go to the next channel in the loop.
                    try
                    {
                        await channel.SendMessageAsync(joinmessage); // Send the join message specified in the Config.json file.
                        return;
                    }
                    catch { continue; } // If it still fails for whatever reason, go to the next channel in the loop.
                }
            }
        }

        private async Task Client_Ready()
        => await Functions.SetBotStatus(Client); // Set bot status from the Config.json file.

        public async Task InitializeAsync()
             => await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services); // Add modules to command service.
    }
}