using Domain.Account;
using Mediator;

namespace Application.Features;

public class GetAccountHandler : IQueryHandler<GetAccountQuery, AccountEntity>
{
    public ValueTask<AccountEntity> Handle(GetAccountQuery query, CancellationToken cancellationToken)
    {
        var account = StaticDb.Accounts.FirstOrDefault(a => a.Id == query.Id);
        if (account == null)
        {
            throw new Exception("Account not found.");
        }

        return ValueTask.FromResult(account);
    }
}
