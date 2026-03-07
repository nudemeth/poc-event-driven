namespace Domain.Account;

public class AccountEntity : Entity<Guid>
{
    private readonly List<DomainEvent> _committedEvents = [];
    private readonly List<DomainEvent> _uncommittedEvents = [];

    private AccountEntity(Guid id) : base(id) { }

    public string AccountHolder { get; private set; } = default!;
    public decimal Balance { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyList<DomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();
    public IReadOnlyList<DomainEvent> CommittedEvents => _committedEvents.AsReadOnly();

    public static AccountEntity Open(string accountHolder, decimal initialDeposit)
    {
        if (string.IsNullOrWhiteSpace(accountHolder))
        {
            throw new ArgumentException("Account holder name is required");
        }

        if (initialDeposit < 0)
        {
            throw new ArgumentException("The initial deposit cannot be negative");
        }

        var bankAccount = new AccountEntity(Guid.NewGuid());
        bankAccount.ApplyUncommittedEvent(new AccountOpened(bankAccount.Id, accountHolder, initialDeposit) { Version = 1 });

        return bankAccount;
    }

    public void Deposit(decimal amount)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Account is closed");
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be positive");
        }

        ApplyUncommittedEvent(new MoneyDeposited(Id, amount) { Version = Version + 1 });
    }

    public void Withdraw(decimal amount)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Account is closed");
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Withdrawal amount must be positive");
        }

        if (Balance - amount < 0)
        {
            throw new InvalidOperationException("Insufficient funds");
        }

        ApplyUncommittedEvent(new MoneyWithdrawn(Id, amount) { Version = Version + 1 });
    }

    public void Transfer(Guid toAccountId, decimal amount)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Account is closed");
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Transfer amount must be positive");
        }

        if (Balance - amount < 0)
        {
            throw new InvalidOperationException("Insufficient funds");
        }

        ApplyUncommittedEvent(new MoneyTransferred(Id, amount, toAccountId) { Version = Version + 1 });
    }

    public void Close()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Account is closed");
        }

        if (Balance != 0)
        {
            throw new InvalidOperationException("Cannot close account with non-zero balance");
        }

        ApplyUncommittedEvent(new AccountClosed(Id) { Version = Version + 1 });
    }

    private void ApplyUncommittedEvent(DomainEvent @event)
    {
        ApplyEventState(@event);
        Version = @event.Version;
        _uncommittedEvents.Add(@event);
    }

    private void ApplyCommittedEvent(DomainEvent @event)
    {
        ApplyEventState(@event);
        Version = @event.Version;
        _committedEvents.Add(@event);
    }

    private void ApplyEventState(DomainEvent @event)
    {
        switch (@event)
        {
            case AccountOpened e:
                AccountHolder = e.AccountHolder;
                Balance = e.InitialDeposit;
                IsActive = true;
                break;
            case MoneyDeposited e:
                Balance += e.Amount;
                break;
            case MoneyWithdrawn e:
                Balance -= e.Amount;
                break;
            case MoneyTransferred e:
                Balance -= e.Amount;
                break;
            case AccountClosed e:
                IsActive = false;
                break;
        }
    }

    public static AccountEntity ReplayEvents(IEnumerable<DomainEvent> events)
    {
        var bankAccount = new AccountEntity(events.First().StreamId);

        foreach (var @event in events)
        {
            bankAccount.ApplyCommittedEvent(@event);
        }

        return bankAccount;
    }
}
