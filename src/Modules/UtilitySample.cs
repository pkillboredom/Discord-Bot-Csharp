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
            double Percentage = (double)Context.Guild.Users.Count(x => x.IsBot) / (double)Context.Guild.MemberCount * 100d; // Calculate the percentage of bots in the server.
            var RoundedPercentage = Math.Round(Percentage, 2); // Round the percentage to 2 digits.

            EmbedBuilder Embed = new EmbedBuilder(); // Declare the embed builder.
            Embed.WithColor(0, 225, 225)
                 .WithDescription(
                $"🏷️\n**Guild name:** {Context.Guild.Name}\n" + // Name of the server.
                $"**Guild ID:** {Context.Guild.Id}\n" + // ID of the server
                $"**Created at:** {Context.Guild.CreatedAt.ToString("dd/M/yyyy", CultureInfo.InvariantCulture)}\n" + // When the server was created.
                $"**Owner:** {Context.Guild.Owner}\n\n" + // Owner of the server.
                $"💬\n" +
                $"**Users:** {Context.Guild.MemberCount - Context.Guild.Users.Count(x => x.IsBot)}\n" + // Amount of members in the server, minus the bots.
                $"**Bots:** {Context.Guild.Users.Count(x => x.IsBot)} [ {RoundedPercentage}% ]\n" + // Amount of bots in the server and the percentage. 
                $"**Channels:** {Context.Guild.Channels.Count}\n" + // Amount of channels.
                $"**Roles:** {Context.Guild.Roles.Count}\n" + // Amount of roles.
                $"**Emotes: ** {Context.Guild.Emotes.Count}\n\n" + // Amount of Emotes.
                $"🌎 **Region:** {Context.Guild.VoiceRegionId}\n\n" + // Server region.
                $"🔒 **Security level:** {Context.Guild.VerificationLevel}") // Verification level of the server.
                 .WithImageUrl(Context.Guild.IconUrl);

            await ReplyAsync($":information_source: Server info for **{Context.Guild.Name}**", embed: Embed.Build()); // Send message and build the embed.
        }

        [Command("role")]
        [Alias("roleinfo")]
        [Summary("Shows information about a role.")]
        public async Task RoleInfo([Remainder]SocketRole role)
        {
            if (role == Context.Guild.EveryoneRole || (role.ToString().Contains("@here"))) return; // Just in case someone tries to be funny.

            await ReplyAsync(
                $":flower_playing_cards: **{role.Name}** information```ini" +
                $"\n[Members]             {role.Members.Count()}" + // Amount of members in the role.
                $"\n[Role ID]             {role.Id}" + // ID of the role.
                $"\n[Hoisted status]      {role.IsHoisted}" + // If the role is hoisted or not.
                $"\n[Created at]          {role.CreatedAt.ToString("dd/M/yyyy", CultureInfo.InvariantCulture)}" + // When the role was made.
                $"\n[Hierarchy position]  {role.Position}" + // The hierarchy position of the role.
                $"\n[Color Hex]           {role.Color}```"); // The color hex of the role.
        }

        [Command("source")]
        [Alias("sourcecode", "src")]
        [Summary("Links the source code used for this bot.")]
        public async Task Source()
            => await ReplyAsync($":heart: **{Context.Client.CurrentUser}** is based on this source code:\nhttps://github.com/VACEfron/Discord-Bot-Csharp");
    }
}
