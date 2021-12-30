using System;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using NLog;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Discord_Bot
{
    public class Program
    {
        static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                var configBuilder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("config.json", optional: false);

                IConfiguration config = configBuilder.Build();

                var services = new ServiceCollection()
                    .AddSingleton(config)
                    .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                    {
                        MessageCacheSize = 500,
                        LogLevel = LogSeverity.Info
                    }))
                    .AddSingleton(new CommandService(new CommandServiceConfig
                    {
                        LogLevel = LogSeverity.Info,
                        DefaultRunMode = RunMode.Async,
                        CaseSensitiveCommands = false
                    }))
                    .AddSingleton<CommandHandlingService>()
                    .AddLogging(loggingBuilder =>
                    {
                        // configure Logging with NLog
                        loggingBuilder.ClearProviders();
                        loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                        loggingBuilder.AddNLog(config);
                    })
                    //.AddSingleton(typeof(Modules.MinecraftModule.MinecraftUtil))
                    .BuildServiceProvider();

                new DiscordBot(services).MainAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }

        }

        
    }
}