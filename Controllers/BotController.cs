using BotSocialMedia.Dtos;
using BotSocialMedia.Services;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "VerifiedOnly")]
public class BotController : ControllerBase
{

    private readonly BotService _botService;


    public BotController(BotService botsService)
    {
        _botService = botsService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        var bots = await _botService.GetAllBots(pageNumber, pageSize);
        return Ok(new
        {
            status = 200,
            PageNumber = pageNumber,
            data = bots
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var botId))
            throw new HttpException("Invalid bot id", 400);

        var bot = await _botService.GetDetail(botId);
        return Ok(new
        {
            status = 200,
            data = bot
        });
    }

    [HttpPost("create")]
    public async Task<IActionResult> Store([FromForm] BotCreateDto dto)
    {

        var bot = await _botService.CreateBot(dto);

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

        var bot = await _botService.UpdateBot(botId, dto);

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
