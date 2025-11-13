public class BankAccount()
{
    public Guid Id { get; private set; }
    public string AccountHolder { get; private set; }
    public decimal Balance { get; private set; }
    public bool IsActive { get; private set; }
    public List<DomainEvent> Events { get; }

    public static BankAccount Open(string accountHolder, decimal initialDeposit)
    {
        if (string.IsNullOrWhiteSpace(accountHolder))
        {
            throw new ArgumentException("Account holder name is required");
        }

        if (initialDeposit < 0)
        {
            throw new ArgumentException("The initial deposit cannot be negative");
        }

        var bankAccount = new BankAccount();
        var @event = new AccountOpened(Guid.NewGuid(), accountHolder, initialDeposit);

        bankAccount.Apply(@event);

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

        Apply(new MoneyDeposited(Id, amount));
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

        Apply(new MoneyWithdrawn(Id, amount));
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

        Apply(new MoneyTransferred(Id, amount, toAccountId));
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

        Apply(new AccountClosed(Id));
    }

    private void Apply(DomainEvent @event)
    {
        switch (@event)
        {
            case AccountOpened e:
                Id = e.AccountId;
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

        Events.Add(@event);
    }

    public static BankAccount ReplayEvents(IEnumerable<DomainEvent> events)
    {
        var bankAccount = new BankAccount();
        foreach (var @event in events)
        {
            bankAccount.Apply(@event);
        }
        return bankAccount;
    }
}
