// Repositories/AccountRepository.cs
using BotSocialMedia.Data;
using BotSocialMedia.Models;
using BotSocialMedia.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BotSocialMedia.Repositories
{
    public class AccountRepository : GenericRepository<Accounts>, IAccountRepository
    {
        public AccountRepository(AppDbContext context) : base(context) { }

        public async Task<bool> IsAccountVerified(Guid accountId)
        {
            return await _dbSet
                            .Where(a => a.Id == accountId)
                            .Select(a => a.IsVerified)
                            .FirstOrDefaultAsync();
        }
    }
}
