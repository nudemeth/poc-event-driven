using Domain;
using Domain.Account;

public interface IAccountRepository
{
    public Task SaveAsync(AccountEntity account);
    public Task SaveAsync(IEnumerable<AccountEntity> accounts);
    public Task<AccountEntity?> GetAccountByIdAsync(Guid id);
}