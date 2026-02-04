using Domain.Account;
using Mediator;

public record GetAccountsQuery : IQuery<List<AccountEntity>>;