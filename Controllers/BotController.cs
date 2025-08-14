using BotSocialMedia.Dtos;
using BotSocialMedia.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class BotController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly BotService _botService;
    private readonly CloudinaryService _cloudinaryService;

    public BotController(BotService botsService, IConfiguration config, CloudinaryService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;

        _config = config;
        _botService = botsService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var bots = await _botService.GetAllBots();
        return Ok(new
        {
            status = 200,
            data = bots
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var botId))
            throw new HttpException("Invalid bot id", 400);

        var bot = await _botService.GetBotById(botId);
        return Ok(new
        {
            status = 200,
            data = bot
        });
    }

    [HttpPost("create")]
    public async Task<IActionResult> Store([FromForm] BotCreateDto dto)
    {
        string? avatarUrl = null;

        if (dto.Avatar != null)
        {
            avatarUrl = await _cloudinaryService.UploadImageAsync(dto.Avatar);
        }

        var bot = await _botService.CreateBot(dto, avatarUrl);

        return Ok(new
        {
            status = 200,
            message = "Bot successfully created",
            data = bot
        });
    }

    [HttpPatch("update/{id}")]
    public async Task<IActionResult> Update([FromRoute] string id, [FromForm] BotUpdateDto dto)
    {
        if (!Guid.TryParse(id, out var botId))
            throw new HttpException("Invalid bot id", 400);

        string? avatarUrl = null;

        if (dto.Avatar != null)
        {
            avatarUrl = await _cloudinaryService.UploadImageAsync(dto.Avatar);
        }

        var bot = await _botService.UpdateBot(botId, dto, avatarUrl);

        return Ok(new
        {
            status = 200,
            message = "Bot successfully updated",
            data = bot
        });
    }

    [HttpPost("soft-delete/{id}")]
    public async Task<IActionResult> SoftDelete([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var botId))
            throw new HttpException("Invalid bot id", 400);
        var bot = await _botService.SoftDelete(botId);
        return Ok(new
        {
            status = 200,
            message = "Bot successfully soft deleted",
            data = bot
        });
    }

    [HttpPost("force-delete/{id}")]
    public async Task<IActionResult> ForceDelete([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var botId))
            throw new HttpException("Invalid bot id", 400);
        var bot = await _botService.ForceDelete(botId);
        return Ok(new
        {
            status = 200,
            message = "Bot successfully force deleted",
            data = bot
        });
    }

}
