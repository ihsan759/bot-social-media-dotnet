using BotSocialMedia.Models;

namespace BotSocialMedia.Repositories.Interfaces
{
    public interface IAccountRepository : IGenericRepository<Accounts>
    {
        Task<Accounts?> GetAccountByEmail(string email);
    }
}
