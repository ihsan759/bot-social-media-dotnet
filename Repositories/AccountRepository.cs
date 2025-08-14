// Repositories/AccountRepository.cs
using BotSocialMedia.Data;
using BotSocialMedia.Models;
using BotSocialMedia.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BotSocialMedia.Repositories
{
    public class AccountRepository : GenericRepository<Accounts>, IAccountRepository
    {
        public AccountRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Accounts?> GetAccountByEmail(string email)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        }
    }
}
