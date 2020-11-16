using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Discord_Bot
{
    public static class Functions
    {
        public static async Task SetBotStatusAsync(DiscordSocketClient client)
        {
            JObject config = GetConfig();

            string currently = config["currently"]?.Value<string>().ToLower();
            string statusText = config["playing_status"]?.Value<string>();
            string onlineStatus = config["status"]?.Value<string>().ToLower();

            // Set the online status
            if (onlineStatus != null && onlineStatus != string.Empty)
            {
                UserStatus userStatus = onlineStatus switch
                {
                    "dnd" => UserStatus.DoNotDisturb,
                    "idle" => UserStatus.Idle,
                    "offline" => UserStatus.Invisible,
                    _ => UserStatus.Online
                };

                await client.SetStatusAsync(userStatus);
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} | Online status set | {userStatus}");
            }

            // Set the playing status
            if (currently != null && currently != string.Empty && 
                statusText != null && statusText != string.Empty)
            {
                ActivityType activity = currently switch
                {
                    "listening" => ActivityType.Listening,
                    "watching" => ActivityType.Watching,
                    "streaming" => ActivityType.Streaming,
                    _ => ActivityType.Playing
                };

                await client.SetGameAsync(statusText, type: activity);
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} | Playing status set | {activity}: {statusText}");
            }            
        }

        public static JObject GetConfig()
        {
            // Get the config file.
            using StreamReader configjson = new StreamReader(Directory.GetCurrentDirectory() + @"/Config.json");
                return (JObject)JsonConvert.DeserializeObject(configjson.ReadToEnd());
        }

        public static string GetAvatarUrl(SocketUser user)
        {
            // Get user avatar and resize it 1024x1024. If the user has no avatar, get the default Discord avatar.
            return user.GetAvatarUrl(size: 1024) ?? user.GetDefaultAvatarUrl(); 
        }
    }
}
