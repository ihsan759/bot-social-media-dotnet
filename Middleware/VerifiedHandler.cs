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

        var account = await _accountRepository.GetById(accountId);
        if (account != null && account.IsVerified)
        {
            context.Succeed(requirement);
        }
    }
}
