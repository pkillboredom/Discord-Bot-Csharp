using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Globalization;

namespace Discord_Bot
{
    public class UtilitySample : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        [Summary("Show current latency.")]
        public async Task Ping()
            => await ReplyAsync($"Latency: {Context.Client.Latency} ms");

        [Command("avatar")]
        [Alias("getavatar")]
        [Summary("Get a user's avatar.")]
        public async Task GetAvatar([Remainder]SocketGuildUser user = null)
        {
            if (user is null)           
                user = Context.User as SocketGuildUser; // If there is no user specified, get the avatar of the user who triggered the command.
            
            await ReplyAsync($":frame_photo: **{user.Username}**'s avatar\n{Functions.GetAvatarUrl(user)}"); // Reply with the user's avatar URL.
        }

        [Command("info")]
        [Alias("server", "serverinfo")]
        [Summary("Show server information.")]
        [RequireBotPermission(GuildPermission.EmbedLinks)] // Require the bot the have the 'embed links' permission to execute this command.
        public async Task ServerEmbed()
        {
            double Percentage = Context.Guild.Users.Count(x => x.IsBot) / Context.Guild.MemberCount * 100d; // Calculate the percentage of bots in the server.
            double RoundedPercentage = Math.Round(Percentage, 2); // Round the percentage to 2 digits.

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(0, 225, 225)
                 .WithDescription(
                $"🏷️\n**Guild name:** {Context.Guild.Name}\n" +
                $"**Guild ID:** {Context.Guild.Id}\n" +
                $"**Created at:** {Context.Guild.CreatedAt.ToString("dd/M/yyyy", CultureInfo.InvariantCulture)}\n" +
                $"**Owner:** {Context.Guild.Owner}\n\n" +
                $"💬\n" +
                $"**Users:** {Context.Guild.MemberCount - Context.Guild.Users.Count(x => x.IsBot)}\n" +
                $"**Bots:** {Context.Guild.Users.Count(x => x.IsBot)} [ {RoundedPercentage}% ]\n" +
                $"**Channels:** {Context.Guild.Channels.Count}\n" +
                $"**Roles:** {Context.Guild.Roles.Count}\n" + 
                $"**Emotes: ** {Context.Guild.Emotes.Count}\n\n" + 
                $"🌎 **Region:** {Context.Guild.VoiceRegionId}\n\n" + 
                $"🔒 **Security level:** {Context.Guild.VerificationLevel}") 
                 .WithImageUrl(Context.Guild.IconUrl);

            await ReplyAsync($":information_source: Server info for **{Context.Guild.Name}**", embed: embed.Build());
        }

        [Command("role")]
        [Alias("roleinfo")]
        [Summary("Show information about a role.")]
        public async Task RoleInfo([Remainder]SocketRole role)
        {
            if (role == Context.Guild.EveryoneRole) return; // Just in case someone tries to be funny.

            await ReplyAsync(
                $":flower_playing_cards: **{role.Name}** information```ini" +
                $"\n[Members]             {role.Members.Count()}" +
                $"\n[Role ID]             {role.Id}" +
                $"\n[Hoisted status]      {role.IsHoisted}" +
                $"\n[Created at]          {role.CreatedAt.ToString("dd/M/yyyy", CultureInfo.InvariantCulture)}" +
                $"\n[Hierarchy position]  {role.Position}" +
                $"\n[Color Hex]           {role.Color}```");
        }

        [Command("source")]
        [Alias("sourcecode", "src")]
        [Summary("Link the source code used for this bot.")]
        public async Task Source()
            => await ReplyAsync($":heart: **{Context.Client.CurrentUser}** is based on this source code:\nhttps://github.com/VACEfron/Discord-Bot-Csharp");
        // Please don't remove this command. I will appreciate it a lot <3
    }
}
