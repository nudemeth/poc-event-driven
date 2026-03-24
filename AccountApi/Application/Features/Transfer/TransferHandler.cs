using Mediator;
using Domain.Account;

namespace Application.Features;

public class TransferHandler : ICommandHandler<TransferCommand, AccountEntity>
{
    private readonly IAccountRepository _accountRepository;
    private readonly TransferService _transferService;

    public TransferHandler(IAccountRepository accountRepository, TransferService transferService)
    {
        _accountRepository = accountRepository;
        _transferService = transferService;
    }

    public async ValueTask<AccountEntity> Handle(TransferCommand command, CancellationToken cancellationToken)
    {
        var sourceAccount = await _accountRepository.GetAccountByIdAsync(command.Id);
        var targetAccount = await _accountRepository.GetAccountByIdAsync(command.ToAccountNumber);

        if (sourceAccount == null || targetAccount == null)
        {
            throw new Exception("One or both accounts not found.");
        }

        try
        {
            _transferService.Transfer(sourceAccount, targetAccount, command.Amount);
            await _accountRepository.SaveAsync([sourceAccount, targetAccount]);
            return sourceAccount;
        }
        catch (Exception ex)
        {
            throw new Exception("Transfer failed: " + ex.Message);
        }
    }
}
