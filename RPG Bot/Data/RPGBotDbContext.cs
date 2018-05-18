using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;

namespace RPG_Bot.Data
{
    public class RPGBotDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        public DbSet<Character> Characters { get; set; }

        public RPGBotDbContext()
            : base()
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer($"Data Source=.\\SQLEXPRESS;AttachDbFilename={Path.GetFullPath(@".\Data\RPGBotDb.mdf")};Integrated Security=True;User Instance=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");
                entity.HasMany(x => x.Characters)
                    .WithOne(x => x.Account)
                    .HasForeignKey("AccountID");
            });

            modelBuilder.Entity<Character>(entity =>
            {
                entity.ToTable("Character");
                entity.HasOne(x => x.Account)
                    .WithMany(x => x.Characters)
                    .HasForeignKey("AccountID");
            });
        }
    }

    public partial class Account
    {
        [NotMapped()]
        public ulong DiscordID { get =>(ulong)this.AccountID; set => this.AccountID = (long)value; }

        public long AccountID { get; set; }

        public int? CharacterID { get; set; }

        [NotMapped]
        public Character ActiveCharacter { get => Characters.SingleOrDefault(x => x.CharacterID == this.CharacterID); }

        public List<Character> Characters { get; set; }
    }

    public partial class Character
    {
        public int CharacterID { get; set; }

        public long AccountID { get; set; }

        public Account Account { get; set; }

        [StringLength(50)]
        public String Name { get; set; }

        public int Level { get; set; }

        public long Experience { get; set; }

        public bool IsActiveCharacter { get; set; }
    }
}
