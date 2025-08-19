using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SSDServer.Database
{
    public partial class ClientDB : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        private static ClientDB instance;

        public static ClientDB Instance
        {
            get
            {
                if (instance == null)
                    instance = new ClientDB();
                return instance;
            }
        }

        private ClientDB()
        {
            this.Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionStringHandler.Instance["CLIENT_DB"]);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity => {
                entity.HasKey(a => a.ID);
                entity.Property(a => a.Username);
                entity.Property(a => a.PasswordHash);
                entity.Property(a => a.Permissions);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}