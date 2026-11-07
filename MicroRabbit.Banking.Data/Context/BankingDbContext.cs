using System;
using System.Collections.Generic;
using System.Text;
using MicroRabbit.Banking.Domain;
using Microsoft.EntityFrameworkCore;


namespace MicroRabbit.Banking.Data.Context
{
    public class BankingDbContext : DbContext
    {
        public BankingDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
    }
}
