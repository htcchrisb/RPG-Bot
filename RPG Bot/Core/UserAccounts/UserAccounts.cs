using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace RPG_Bot.Core.UserAccounts
{
    public static class UserAccounts
    {
        private static List<UserAccount> accounts;

        private static string accountsFile = "Resources/accounts.json";

        static UserAccounts()
        {
            if (DataStorage.SaveExists(accountsFile))
            {
                accounts = DataStorage.LoadUserAccounts(accountsFile).ToList();
            }
            else
            {
                accounts = new List<UserAccount>();
                SaveAccounts();
            }
        }

        /*static UserAccounts()
        {
            bool exists = false;
            try { exists = DataStorage.SaveExists(accountsFile); }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw new InvalidOperationException("Error when checking for accountsFile.");
            }

            if(exists)
            {
                try { accounts = DataStorage.LoadUserAccounts(accountsFile).ToList(); }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw new InvalidOperationException("Error when trying to load the accountsFile.");
                }
            }

            else
            {
                accounts = new List<UserAccount>();
                SaveAccounts();
            }
        }*/

        public static void SaveAccounts()
        {
            DataStorage.SaveUserAccounts(accounts, accountsFile);
        }

        public static UserAccount GetAccount(SocketUser user)
        {
            return GetOrCreateAccount(user.Id);
        }

        private static UserAccount GetOrCreateAccount(ulong id)
        {
            var result = from a in accounts
                         where a.ID == id
                         select a;

            var account = result.FirstOrDefault();
            if (account == null) account = CreateUserAccount(id);
            return account;
        }

        private static UserAccount CreateUserAccount(ulong id)
        {
            var newAccount = new UserAccount()
            {
                ID = id,
                Level = 0,
                EXP = 0
            };

            accounts.Add(newAccount);
            SaveAccounts();
            return newAccount;
        }

    }
}
