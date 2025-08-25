// Repositories/AccountRepository.cs
using BotSocialMedia.Data;
using BotSocialMedia.Models;
using BotSocialMedia.Repositories.Interfaces;

namespace BotSocialMedia.Repositories
{
    public class AccountRepository : GenericRepository<Accounts>, IAccountRepository
    {
        public AccountRepository(AppDbContext context) : base(context) { }

    }
}
