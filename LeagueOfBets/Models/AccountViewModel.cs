using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeagueOfBets.Models
{
    public class AccountViewModel
    {
        public string Email { get; }

        public long Balance { get; }

        public AccountViewModel(string email, long balance)
        {
            Email = email;
            Balance = balance;
        }
    }
}
