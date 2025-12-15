using Domain.Account;

public class StaticDb
{
    public static List<AccountEntity> Accounts = new()
    {
        AccountEntity.Open("Alice", 1000),
    };
}