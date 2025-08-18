using BotSocialMedia.Dtos;
using BotSocialMedia.Models;
using BotSocialMedia.Repositories.Interfaces;

namespace BotSocialMedia.Services
{
    public class BotService
    {
        private readonly IBotRepository _botsRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CloudinaryService _cloudinaryService;

        public BotService(IBotRepository botsRepository, IAccountRepository accountRepository, IHttpContextAccessor httpContextAccessor, CloudinaryService cloudinaryService)
        {
            _botsRepository = botsRepository;
            _accountRepository = accountRepository;
            _httpContextAccessor = httpContextAccessor;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<List<Bots>> GetAllBots()
        {
            var id = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            var role = _httpContextAccessor.HttpContext?.User.FindFirst("role")?.Value;
            if (role == "admin")
                return await _botsRepository.GetAll<Bots>();

            return await _botsRepository.GetAll<Bots>(x => x.AccountId.ToString() == id);
        }

        public async Task<Bots?> GetBotById(Guid id)
        {
            var idj = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            var role = _httpContextAccessor.HttpContext?.User.FindFirst("role")?.Value;
            var bot = await _botsRepository.GetById(id) ?? throw new HttpException("Bot not found", 404);
            if (bot.AccountId.ToString() != idj && role != "admin")
                throw new HttpException("Forbidden: You can only access your own bots", 403);

            return bot;
        }

        public async Task<Bots> CreateBot(BotCreateDto bot, string? avatarUrl = null)
        {
            var id = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            var account = await _accountRepository.Exists(x => x.Id == Guid.Parse(id!));
            if (!account)
            {
                throw new HttpException($"Account with ID {id} does not exist.", 400);
            }
            // Map DTO to entity
            var botEntity = new Bots
            {
                AccountId = Guid.Parse(id!),
                AvatarUrl = avatarUrl,
                Name = bot.Name,
                Token = bot.Token,
                ExpiresAt = bot.ExpiresAt,
                Phone = bot.Phone
            };

            return await _botsRepository.Create(botEntity);
        }

        public async Task<Bots?> UpdateBot(Guid id, BotUpdateDto updatedData, string? url)
        {
            var idj = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            var existingBot = await _botsRepository.GetById(id) ?? throw new HttpException("Bot not found", 404);
            if (existingBot.AccountId.ToString() != idj)
                throw new HttpException("Forbidden: You can only update your own bots", 403);

            existingBot.Name = updatedData.Name ?? existingBot.Name;
            existingBot.Token = updatedData.Token ?? existingBot.Token;
            existingBot.ExpiresAt = updatedData.ExpiresAt ?? existingBot.ExpiresAt;
            existingBot.Phone = updatedData.Phone ?? existingBot.Phone;
            existingBot.AvatarUrl = url ?? existingBot.AvatarUrl;
            existingBot.Active = updatedData.Active ?? existingBot.Active;
            existingBot.Status = updatedData.Status ?? existingBot.Status;


            existingBot.UpdatedAt = DateTime.UtcNow;

            await _botsRepository.Update(existingBot);
            return existingBot;
        }

        public async Task<bool> ForceDelete(Guid id)
        {
            var bot = await _botsRepository.GetById(id) ?? throw new HttpException("Bot not found", 404);

            // check if already soft deleted
            if (bot.DeletedAt == null)
                throw new HttpException("Forbidden: You must soft delete the bot before force deleting", 403);

            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            var role = _httpContextAccessor.HttpContext?.User.FindFirst("role")?.Value;
            if (userId != bot.AccountId.ToString() && role != "admin")
                throw new HttpException("Forbidden", 403);

            if (bot.AvatarUrl != null)
            {    // Delete the avatar image from Cloudinary
                var publicId = _cloudinaryService.ExtractPublicId(bot.AvatarUrl);
                var deleted = await _cloudinaryService.DeleteImageAsync(publicId);

                if (!deleted)
                    throw new HttpException("Failed to delete avatar image. Bot cannot be deleted.", 500);

            }

            return await _botsRepository.Delete(id);
        }

        public async Task<bool> SoftDelete(Guid id)
        {
            var bot = await _botsRepository.GetById(id) ?? throw new HttpException("Bot not found", 404);
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            var role = _httpContextAccessor.HttpContext?.User.FindFirst("role")?.Value;
            if (bot == null || (userId != bot.AccountId.ToString() && role != "admin")) throw new HttpException("Forbidden", 403);

            // Update the bot in the repository
            await _botsRepository.Update(id, bo =>
            {
                bo.DeletedAt = DateTime.UtcNow;
            });
            return true;
        }
    }
}
