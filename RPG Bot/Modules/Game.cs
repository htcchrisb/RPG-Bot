using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using RPG_Bot.Core.UserAccounts;

namespace RPG_Bot.Modules
{
    public class Game : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        // [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RevealSecret([Remainder]string arg = "")
        {
            if (!UserIsSecretOwner((SocketGuildUser)Context.User))
            {
                await Context.Channel.SendMessageAsync(":x: You need the proper role to use this command. " + Context.User.Mention);
                return;
            }
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
            await dmChannel.SendMessageAsync(Utilities.GetAlert("help"));

        }
        private bool UserIsSecretOwner(SocketGuildUser user)
        {
            string targetRoleName = "Bot User";
            var result = from r in user.Guild.Roles
                         where r.Name == targetRoleName
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return false;
            var targetRole = user.Guild.GetRole(roleID);
            return user.Roles.Contains(targetRole);

        }

        [Command("stats")]
        //[RequireUserPermission(GuildPermission.)]
        public async Task MyStats([Remainder] string arg = "")
        {
            SocketUser target = null;
            var mentionedUser = Context.Message.MentionedUsers.FirstOrDefault();
            target = mentionedUser ?? Context.User;

            var account = UserAccounts.GetAccount(target);
            var embed = new EmbedBuilder();
            string avatarUrlValue = mentionedUser.GetAvatarUrl(ImageFormat.Auto, 512);
            embed.WithTitle($"{target.Mention}'s Character Stats");
            embed.WithDescription($"Level:{account.Level}");
            embed.WithThumbnailUrl(avatarUrlValue);
            embed.WithColor(new Color(152, 235, 117));



            await Context.Channel.SendMessageAsync($"{target.Mention} is Level {account.Level} and has {account.EXP} exp.");
        }
        
    }
}
