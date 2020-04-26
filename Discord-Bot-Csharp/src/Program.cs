using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Discord_Bot
{
    public class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            using (var services = ConfigureServices())
            {
                Console.WriteLine("Ready for takeoff...");
                var Client = services.GetRequiredService<DiscordSocketClient>();

                Client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                using (StreamReader configjson = new StreamReader(Directory.GetCurrentDirectory() + @"/Config.json")) // Get the config file. Note: this gets the current directory and removes the '\bin\Debug' or '\bin\Release' part and replaces it with 'Config.json'. Your directory may differ.
                {
                    var readJSON = configjson.ReadToEnd(); // Read the Config.json file.
                    var config = (JObject)JsonConvert.DeserializeObject(readJSON); // Deserialize the JSON.
                    string Token = config["token"].Value<string>(); // Get the bot token from the Config.json file.

                    await Client.LoginAsync(TokenType.Bot, Token); // Login the bot to Discord.
                    await Client.StartAsync(); // Start the bot.

                    await services.GetRequiredService<CommandHandlingService>().InitializeAsync(); // Initialize the command handling service.
                    await Task.Delay(-1); // Run the bot forever.
                }                    
            }
        }

        public ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig // Add Discord to the collection.
                { 
                    MessageCacheSize = 250, // Enable message cache.
                    LogLevel = LogSeverity.Info // Sets log level.
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig // Add the command service to the collection.
                { 
                    LogLevel = LogSeverity.Info, // Sets log level.
                    DefaultRunMode = RunMode.Async, // Run all commands asynchronously. 
                    CaseSensitiveCommands = false // Make command case insensitive.
                }))
                .AddSingleton<CommandHandlingService>() // Add command handler to the service.
                .BuildServiceProvider(); // Build the service provider.
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}