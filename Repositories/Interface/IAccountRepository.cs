using BotSocialMedia.Models;

namespace BotSocialMedia.Repositories.Interfaces
{
    public interface IAccountRepository : IGenericRepository<Accounts>
    {
        Task<bool> IsAccountVerified(Guid accountId);
    }
}
