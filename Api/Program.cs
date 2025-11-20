using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var accounts = new List<BankAccount>();

app.MapGet("/accounts", () =>
{
    return Results.Ok(accounts.Where(a => a.IsActive));
});

app.MapPost("/accounts", ([FromBody] CreateAccountCommand account) =>
{
    var newAccount = BankAccount.Open(account.AccountHolder, account.InitialDeposit);
    accounts.Add(newAccount);
    return Results.Created($"/accounts/{newAccount.Id}", newAccount);
});

app.MapGet("/accounts/{id}", (Guid id) =>
{
    var account = accounts.FirstOrDefault(a => a.Id == id);
    return account != null ? Results.Ok(account) : Results.NotFound();
});

app.MapPost("/accounts/{id}/deposit", (Guid id, [FromBody] decimal amount) =>
{
    var account = accounts.FirstOrDefault(a => a.Id == id);
    if (account == null)
    {
        return Results.NotFound();
    }

    try
    {
        account.Deposit(amount);
        return Results.Ok(account);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/accounts/{id}/withdraw", (Guid id, [FromBody] decimal amount) =>
{
    var account = accounts.FirstOrDefault(a => a.Id == id);
    if (account == null)
    {
        return Results.NotFound();
    }

    try
    {
        account.Withdraw(amount);
        return Results.Ok(account);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/accounts/{id}/transfer", (Guid id, [FromBody] Guid toAccountId, decimal amount) =>
{
    var account = accounts.FirstOrDefault(a => a.Id == id);
    var toAccount = accounts.FirstOrDefault(a => a.Id == toAccountId);
    if (account == null || toAccount == null)
    {
        return Results.NotFound();
    }

    try
    {
        account.Transfer(toAccountId, amount);
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
