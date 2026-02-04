using Mediator;
using Domain.Account;

namespace Application.Features;

public class TransferHandler : ICommandHandler<TransferCommand, AccountEntity>
{
    private readonly IAccountRepository _accountRepository;

    public TransferHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async ValueTask<AccountEntity> Handle(TransferCommand command, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetAccountByIdAsync(command.Id);
        var toAccount = await _accountRepository.GetAccountByIdAsync(command.ToAccountNumber);
        if (account == null || toAccount == null)
        {
            throw new Exception("One or both accounts not found.");
        }

        try
        {
            account.Transfer(command.ToAccountNumber, command.Amount);
            await _accountRepository.SaveAsync(account);
            return account;
        }
        catch (Exception ex)
        {
            throw new Exception("Transfer failed: " + ex.Message);
        }
    }
}
