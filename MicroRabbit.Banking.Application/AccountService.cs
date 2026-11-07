using System;
using System.Collections.Generic;
using System.Text;
using MicroRabbit.Banking.Domain;

namespace MicroRabbit.Banking.Application
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;


        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public IEnumerable<Account> GetAccounts()
        {
            return _accountRepository.GetAccounts();
        }
    }

}
