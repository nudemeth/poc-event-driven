using Domain.Account;
using Mediator;

public class GetAccountsHandler : IQueryHandler<GetAccountsQuery, List<AccountEntity>>
{
    public ValueTask<List<AccountEntity>> Handle(GetAccountsQuery query, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(StaticDb.Accounts.Where(a => a.IsActive).ToList());
    }
}
