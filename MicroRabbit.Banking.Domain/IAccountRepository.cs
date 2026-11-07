using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Banking.Domain
{
    public interface IAccountRepository
    {
        IEnumerable<Account> GetAccounts();
    }
}
