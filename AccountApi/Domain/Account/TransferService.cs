namespace Domain.Account;

public class TransferService
{
    public void Transfer(AccountEntity sourceAccount, AccountEntity targetAccount, decimal amount)
    {
        if (sourceAccount == null)
        {
            throw new ArgumentNullException(nameof(sourceAccount));
        }

        if (targetAccount == null)
        {
            throw new ArgumentNullException(nameof(targetAccount));
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Transfer amount must be positive");
        }

        sourceAccount.TransferOut(targetAccount.Id, amount);
        targetAccount.TransferIn(sourceAccount.Id, amount);
    }
}
