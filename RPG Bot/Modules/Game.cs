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
using RPG_Bot.Data;
using Microsoft.EntityFrameworkCore;

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

        /// <summary>
        /// This method is an alternative to the MyStats method above. This one uses the database file
        /// as it's data storage mechanism.
        /// </summary>
        /// <param name="arg">extra arguments that might be supplied by the sender of the command.</param>
        /// <returns></returns>
        [Command("stats2")]
        public async Task CharacterStats([Remainder] String arg = "")
        {
            var target = SelectTarget(Context);
            var account = GetOrCreateAccount(target);
            var character = GetOrCreateCharacter(account);
            var embed = CreateStatsPanel(target, account, character);

            await Context.Channel.SendMessageAsync(null, embed: embed);
        }

        private SocketUser SelectTarget(SocketCommandContext context)
        {
            var mentionedUser = Context.Message.MentionedUsers.FirstOrDefault();
            return mentionedUser ?? Context.User;
        }

        /// <summary>
        /// This method either retrieves an existing Account record or creates an Account.
        /// </summary>
        /// <param name="target">The Discord target whose Account we are trying to retrieve.</param>
        /// <returns></returns>
        private Account GetOrCreateAccount(SocketUser target)
        {
            Account acct = default(Account);
            using (RPGBotDbContext context = new RPGBotDbContext())
            {
                // We have to cast the Discord Id to a long because Entity Framework/Sql Server does not support unsigned integers.
                acct = context.Accounts
                    .Include(x => x.Characters)
                    .SingleOrDefault(x => x.AccountID == (long)target.Id);
                if (acct == null)
                {
                    acct = new Account()
                    {
                        AccountID = (long)target.Id,
                    };
                    context.Accounts.Add(acct);
                    context.SaveChanges();
                }
            }
            return acct;
        }

        /// <summary>
        /// This method either retrieves the default Character associated with this account or
        /// creates a new character to be that default.
        /// </summary>
        /// <param name="acct">The user's account whose character we are trying to retrieve.</param>
        /// <returns></returns>
        private Character GetOrCreateCharacter(Account acct)
        {
            Character chr = default(Character);
            if (acct.ActiveCharacter != null)
            {
                chr = acct.ActiveCharacter;
            }
            else
            {
                using (RPGBotDbContext context = new RPGBotDbContext())
                {
                    context.Attach<Account>(acct);
                    chr = new Character()
                    {
                        Account = acct,
                        AccountID = acct.AccountID,
                        Experience = 0,
                        Level = 1,
                        IsActiveCharacter = true,
                        Name = "[Character Name]"
                    };
                    context.Characters.Add(chr);
                    context.SaveChanges();
                }
            }
            return chr;
        }

        private Embed CreateStatsPanel(SocketUser target, Account account, Character character)
        {
            var embed = new EmbedBuilder();
            string avatarUrlValue = target.GetAvatarUrl(ImageFormat.Auto, 512);
            embed.WithTitle($"{target.Mention}'s Character: {character.Name}");
            // Can embeds have multi-line descriptions?
            embed.WithDescription(
                String.Format(
@"Level: {0}
Experience: {1}",
                    character.Level,
                    character.Experience));
            embed.WithThumbnailUrl(avatarUrlValue);
            embed.WithColor(new Color(152, 235, 117));
            return embed.Build();
        }
    }
}
