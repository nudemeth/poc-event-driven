using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var accounts = new List<BankAccount>
{
    BankAccount.Open("Alice", 1000),
};

app.MapGet("/accounts", () =>
{
    return Results.Ok(accounts.Where(a => a.IsActive));
});

app.MapPost("/accounts", ([FromBody] CreateAccountCommand command) =>
{
    var account = BankAccount.Open(command.AccountHolder, command.InitialDeposit);
    accounts.Add(account);
    return Results.Created($"/accounts/{account.Id}", account);
});

app.MapGet("/accounts/{id}", (Guid id) =>
{
    var account = accounts.FirstOrDefault(a => a.Id == id);
    return account != null ? Results.Ok(account) : Results.NotFound();
});

app.MapPost("/accounts/{id}/deposit", (Guid id, [FromBody] DepositCommand command) =>
{
    var account = accounts.FirstOrDefault(a => a.Id == id);
    if (account == null)
    {
        return Results.NotFound();
    }

    try
    {
        account.Deposit(command.Amount);
        return Results.Ok(account);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/accounts/{id}/withdraw", (Guid id, [FromBody] WithdrawCommand command) =>
{
    var account = accounts.FirstOrDefault(a => a.Id == id);
    if (account == null)
    {
        return Results.NotFound();
    }

    try
    {
        account.Withdraw(command.Amount);
        return Results.Ok(account);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/accounts/{id}/transfer", (Guid id, [FromBody] TransferCommand command) =>
{
    var account = accounts.FirstOrDefault(a => a.Id == id);
    var toAccount = accounts.FirstOrDefault(a => a.Id == command.ToAccountNumber);
    if (account == null || toAccount == null)
    {
        return Results.NotFound();
    }

    try
    {
        account.Transfer(command.ToAccountNumber, command.Amount);
        return Results.Ok(account);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapDelete("/accounts/{id}", (Guid id) =>
{
    var account = accounts.FirstOrDefault(a => a.Id == id);
    if (account == null)
    {
        return Results.NotFound();
    }

    try
    {
        account.Close();
        return Results.Ok(account);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.Run();
