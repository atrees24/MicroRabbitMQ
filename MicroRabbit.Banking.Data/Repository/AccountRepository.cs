using System;
using System.Collections.Generic;
using System.Text;
using MicroRabbit.Banking.Data.Context;
using MicroRabbit.Banking.Domain;
using System.Collections.Generic;
namespace MicroRabbit.Banking.Data.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private BankingDbContext _ctx;

        public AccountRepository(BankingDbContext ctx)
        {
            _ctx = ctx;
        }

        public IEnumerable<Account> GetAccounts()
        {
            return _ctx.Accounts;
        }
    }
}
