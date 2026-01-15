using Domain;
using Domain.Account;

public interface IAccountRepository
{
    public Task AppendAsync<TEvent>(TEvent @event) where TEvent : DomainEvent;
    public Task<AccountEntity?> GetAccountByIdAsync(Guid id);
}