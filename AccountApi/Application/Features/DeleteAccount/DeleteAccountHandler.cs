using Mediator;
using Domain.Account;

namespace Application.Features;

public class DeleteAccountHandler : ICommandHandler<DeleteAccountCommand>
{
    private readonly IAccountRepository _accountRepository;

    public DeleteAccountHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async ValueTask<Unit> Handle(DeleteAccountCommand command, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetAccountByIdAsync(command.Id);
        if (account == null)
        {
            throw new Exception("Account not found.");
        }

        try
        {
            account.Close();
            return Unit.Value;
        }
        catch (Exception ex)
        {
            throw new Exception("Account closure failed: " + ex.Message);
        }
    }
}