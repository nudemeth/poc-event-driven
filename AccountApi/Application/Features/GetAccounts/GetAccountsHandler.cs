using Domain.Account;
using Mediator;

namespace Application.Features;

public class GetAccountsHandler : IQueryHandler<GetAccountsQuery, List<AccountEntity>>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountsHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async ValueTask<List<AccountEntity>> Handle(GetAccountsQuery query, CancellationToken cancellationToken)
    {
        // GetAccountsHandler will need an appropriate repository method to retrieve all accounts
        // This is a placeholder until GetAllActiveAccountsAsync is implemented
        return new List<AccountEntity>();
    }
}
