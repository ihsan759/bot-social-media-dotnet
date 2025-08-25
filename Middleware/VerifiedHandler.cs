using BotSocialMedia.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

public class VerifiedRequirement : IAuthorizationRequirement { }

public class VerifiedHandler : AuthorizationHandler<VerifiedRequirement>
{
    private readonly IAccountRepository _accountRepository;

    public VerifiedHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VerifiedRequirement requirement)
    {
        var idClaim = context.User.FindFirst("id")?.Value;
        if (idClaim == null || !Guid.TryParse(idClaim, out var accountId))
            return;

        var isVerified = await _accountRepository.Exists(x => x.Id == accountId && x.IsVerified);
        if (isVerified)
        {
            context.Succeed(requirement);
        }
        else
        {
            throw new HttpException("Your account is not verified", StatusCodes.Status403Forbidden);
        }
    }
}
