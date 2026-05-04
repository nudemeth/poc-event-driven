using AccountProjectionDataAccess;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.RebuildAccountsProjections;

public class RebuildAccountsProjectionsHandler : ICommandHandler<RebuildAccountsProjectionsCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly AccountProjectionDbContext _dbContext;

    public RebuildAccountsProjectionsHandler(IAccountRepository accountRepository, AccountProjectionDbContext dbContext)
    {
        _accountRepository = accountRepository;
        _dbContext = dbContext;
    }

    public async ValueTask<Unit> Handle(RebuildAccountsProjectionsCommand command, CancellationToken cancellationToken)
    {
        var accountIds = command.AccountIds;

        if (accountIds == null || !accountIds.Any())
        {
            return await ValueTask.FromResult(Unit.Value); // No account IDs provided, nothing to rebuild
        }

        foreach (var accountId in accountIds)
        {
            var account = await _accountRepository.GetAccountByIdAsync(accountId);

            if (account == null)
            {
                continue; // Account not found, skip to next
            }

            var projection = new AccountSummaryProjection
            {
                Id = account.Id,
                AccountHolder = account.AccountHolder,
                Balance = account.Balance,
                IsActive = account.IsActive,
                Version = account.Version
            };

            var existingProjection = await _dbContext.AccountSummaryProjections.FirstOrDefaultAsync(a => a.Id == account.Id);

            if (existingProjection != null)
            {
                _dbContext.AccountSummaryProjections.Remove(existingProjection);
            }

            await _dbContext.AccountSummaryProjections.AddAsync(projection);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return await ValueTask.FromResult(Unit.Value);
    }
}
