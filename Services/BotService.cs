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

        public async Task<IEnumerable<object>> GetAllBots(int pageNumber = 1, int pageSize = 50)
        {
            var id = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            var role = _httpContextAccessor.HttpContext?.User.FindFirst("role")?.Value;
            if (role == "admin")
            {
                var result = await _botsRepository.GetAll(selector: b => new
                {
                    b.Id,
                    b.Name,
                    b.AvatarUrl,
                    b.Platform,
                    b.Status,
                    Account = new
                    {
                        b.Account.Name,
                    }
                },
                pageNumber: pageNumber,
                pageSize: pageSize);
                return result;
            }
            return await _botsRepository.GetAll(predicate: x => x.AccountId.ToString() == id, selector: b => new
            {
                b.Id,
                b.Name,
                b.AvatarUrl,
                b.Platform,
                b.Status
            },
            pageNumber: pageNumber,
            pageSize: pageSize);
        }

        public async Task<object?> GetDetail(Guid id)
        {
            var idj = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            var role = _httpContextAccessor.HttpContext?.User.FindFirst("role")?.Value;

            if (role == "admin")
            {
                var result = await _botsRepository.GetById(id, selector: b => new
                {
                    b.Id,
                    b.Name,
                    b.AvatarUrl,
                    b.Platform,
                    b.Status,
                    Account = new
                    {
                        b.Account.Name,
                        b.Account.Email,
                        b.Account.Phone,
                        b.Account.Status,
                    }
                }) ?? throw new HttpException("Bot not found", 404);
                return result;
            }

            var bot = await _botsRepository.GetById(id) ?? throw new HttpException("Bot not found", 404);

            if (bot!.AccountId.ToString() != idj && role != "admin")
                throw new HttpException("Forbidden: You can only access your own bots", 403);

            return bot;
        }

        public async Task<Bots> CreateBot(BotCreateDto bot)
        {
            var id = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            var account = await _accountRepository.Exists(x => x.Id == Guid.Parse(id!));
            if (!account)
            {
                throw new HttpException($"Account with ID {id} does not exist.", 400);
            }
            string? uploadedAvatarUrl = null;
            try
            {
                if (bot.Avatar != null)
                {
                    uploadedAvatarUrl = await _cloudinaryService.UploadImageAsync(bot.Avatar, CloudinaryFolders.Bots);
                    if (uploadedAvatarUrl == null)
                        throw new HttpException("Failed to upload avatar image", 500);

                }
                // Map DTO to entity
                var botEntity = new Bots
                {
                    AccountId = Guid.Parse(id!),
                    AvatarUrl = uploadedAvatarUrl,
                    Name = bot.Name,
                    Token = bot.Token,
                    ExpiresAt = bot.ExpiresAt,
                    Phone = bot.Phone
                };

                return await _botsRepository.Create(botEntity);
            }
            catch
            {
                if (uploadedAvatarUrl != null)
                {
                    var publicId = _cloudinaryService.ExtractPublicId(uploadedAvatarUrl);
                    await _cloudinaryService.DeleteImageAsync(publicId);
                }
                throw new HttpException("Something went wrong", 500);
            }

        }

        public async Task<Bots?> UpdateBot(Guid id, BotUpdateDto updatedData)
        {
            var idj = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            var existingBot = await _botsRepository.GetById(id) ?? throw new HttpException("Bot not found", 404);
            if (existingBot.AccountId.ToString() != idj)
                throw new HttpException("Forbidden: You can only update your own bots", 403);

            string? uploadedAvatarUrl = null;

            try
            {
                if (updatedData.Avatar != null)
                {
                    uploadedAvatarUrl = await _cloudinaryService.UploadImageAsync(updatedData.Avatar, CloudinaryFolders.Bots);
                    if (uploadedAvatarUrl == null)
                        throw new HttpException("Failed to upload avatar image", 500);
                }
                string? existingAvatarUrl = existingBot.AvatarUrl;

                existingBot.AvatarUrl = uploadedAvatarUrl ?? existingBot.AvatarUrl;
                existingBot.Name = updatedData.Name ?? existingBot.Name;
                existingBot.Token = updatedData.Token ?? existingBot.Token;
                existingBot.ExpiresAt = updatedData.ExpiresAt ?? existingBot.ExpiresAt;
                existingBot.Phone = updatedData.Phone ?? existingBot.Phone;
                existingBot.Active = updatedData.Active ?? existingBot.Active;
                existingBot.Status = updatedData.Status ?? existingBot.Status;


                existingBot.UpdatedAt = DateTime.UtcNow;

                await _botsRepository.Update(existingBot);

                // Delete the old avatar image if it exists
                if (uploadedAvatarUrl != null && existingAvatarUrl != null)
                {
                    var publicId = _cloudinaryService.ExtractPublicId(existingAvatarUrl);
                    await _cloudinaryService.DeleteImageAsync(publicId);
                }
                return existingBot;
            }
            catch
            {
                if (uploadedAvatarUrl != null)
                {
                    var publicId = _cloudinaryService.ExtractPublicId(uploadedAvatarUrl);
                    await _cloudinaryService.DeleteImageAsync(publicId);
                }
                throw new HttpException("Something went wrong", 500);
            }

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
