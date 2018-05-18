using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RPG_Bot.Data;
using System;

namespace RPG_Bot_Tests
{
    [TestClass]
    public class RPBBotDbContext_Tests
    {
        [TestMethod]
        public void TestRPGBotDbContext_AddAccount()
        {
            Random rand = new Random();
            byte[] idBytes = new byte[8];
            rand.NextBytes(idBytes);
            long newID = BitConverter.ToInt64(idBytes, 0);

            EntityState predictedEntityState = EntityState.Unchanged;
            EntityState actualEntityState = EntityState.Detached;
            using (RPGBotDbContext context = new RPGBotDbContext())
            {
                Account newAcct = new Account()
                {
                    AccountID = newID
                };

                context.Accounts.Add(newAcct);
                context.SaveChanges();

                actualEntityState = context.Entry<Account>(newAcct).State;
            }

            Assert.AreEqual(predictedEntityState, actualEntityState);
        }

        [TestMethod]
        public void TestRPGBotDbContext_RetrieveAccount()
        {
            Account acct;
            using (RPGBotDbContext context = new RPGBotDbContext())
            {
                acct = context.Accounts.FirstOrDefaultAsync().Result;
            }

            Assert.IsNotNull(acct);
        }
    }
}