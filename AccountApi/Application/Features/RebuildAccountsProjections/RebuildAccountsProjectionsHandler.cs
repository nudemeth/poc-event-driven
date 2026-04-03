using AccountDataAccess;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.RebuildAccountsProjections;

public class RebuildAccountsProjectionsHandler : ICommandHandler<RebuildAccountsProjectionsCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly AccountDbContext _dbContext;

    public RebuildAccountsProjectionsHandler(IAccountRepository accountRepository, AccountDbContext dbContext)
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

            var projection = new AccountProjection
            {
                Id = account.Id,
                AccountHolder = account.AccountHolder,
                Balance = account.Balance,
                IsActive = account.IsActive,
                Version = account.Version
            };

            var existingProjection = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

            if (existingProjection != null)
            {
                _dbContext.Accounts.Remove(existingProjection);
            }

            await _dbContext.Accounts.AddAsync(projection);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return await ValueTask.FromResult(Unit.Value);
    }
}
