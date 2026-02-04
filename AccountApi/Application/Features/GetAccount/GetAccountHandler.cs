using Domain.Account;
using Mediator;

namespace Application.Features;

public class GetAccountHandler : IQueryHandler<GetAccountQuery, AccountEntity>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async ValueTask<AccountEntity> Handle(GetAccountQuery query, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetAccountByIdAsync(query.Id);
        if (account == null)
        {
            throw new Exception("Account not found.");
        }

        return account;
    }
}
