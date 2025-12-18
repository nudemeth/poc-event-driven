using Domain.Account;
using Mediator;

namespace Application.Features;

public record GetAccountQuery(Guid Id) : IQuery<AccountEntity>;
