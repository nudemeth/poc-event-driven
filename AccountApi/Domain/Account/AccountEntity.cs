namespace Domain.Account;

public class AccountEntity : Entity<Guid>
{
    private AccountEntity(Guid id) : base(id) { }

    public string AccountHolder { get; private set; } = default!;
    public decimal Balance { get; private set; }
    public bool IsActive { get; private set; }

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

    public void TransferOut(Guid toAccountId, decimal amount)
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

        ApplyUncommittedEvent(new MoneyTransferredOut(Id, amount, toAccountId) { Version = Version + 1 });
    }

    public void TransferIn(Guid fromAccountId, decimal amount)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Account is closed");
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Transfer amount must be positive");
        }

        ApplyUncommittedEvent(new MoneyTransferredIn(Id, fromAccountId, amount) { Version = Version + 1 });
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

    protected override void ApplyEventState(DomainEvent @event)
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
            case MoneyTransferredOut e:
                Balance -= e.Amount;
                break;
            case MoneyTransferredIn e:
                Balance += e.Amount;
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

    /// <summary>
    /// Creates a snapshot of the current entity state.
    /// </summary>
    public override Snapshot CreateSnapshot()
    {
        return new AccountSnapshot(
            AccountId: Id,
            Version: Version,
            AccountHolder: AccountHolder,
            Balance: Balance,
            IsActive: IsActive
        );
    }

    /// <summary>
    /// Restores an entity from a snapshot and replays subsequent events.
    /// </summary>
    public static AccountEntity RestoreFromSnapshot(AccountSnapshot snapshot, IEnumerable<DomainEvent> eventsAfterSnapshot)
    {
        var bankAccount = new AccountEntity(snapshot.AccountId)
        {
            AccountHolder = snapshot.AccountHolder,
            Balance = snapshot.Balance,
            IsActive = snapshot.IsActive,
            Version = snapshot.Version
        };

        foreach (var @event in eventsAfterSnapshot)
        {
            bankAccount.ApplyCommittedEvent(@event);
        }

        return bankAccount;
    }
}
