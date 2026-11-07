using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using MicroRabbit.Banking.Domain;

namespace MicroRabbit.Banking.Application
{
    public interface IAccountService
    {
        IEnumerable<Account> GetAccounts();
       
    }
}
