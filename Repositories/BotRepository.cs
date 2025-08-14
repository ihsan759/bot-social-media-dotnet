// Repositories/AccountRepository.cs
using BotSocialMedia.Data;
using BotSocialMedia.Models;
using BotSocialMedia.Repositories.Interfaces;

namespace BotSocialMedia.Repositories
{
    public class BotRepository : GenericRepository<Bots>, IBotRepository
    {
        public BotRepository(AppDbContext context) : base(context)
        {
        }
    }
}
