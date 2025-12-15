using Application.Features;
using Domain.Account;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var accounts = new List<AccountEntity>
{
    AccountEntity.Open("Alice", 1000),
};

app.MapGet("/accounts", () =>
{
    return Results.Ok(accounts.Where(a => a.IsActive));
});

app.MapPost("/accounts", ([FromBody] CreateAccountCommand command) =>
{

});

app.MapGet("/accounts/{id}", (Guid id) =>
{
    var account = accounts.FirstOrDefault(a => a.Id == id);
    return account != null ? Results.Ok(account) : Results.NotFound();
});

app.MapPost("/accounts/{id}/deposit", (Guid id, [FromBody] DepositCommand command) =>
{

});

app.MapPost("/accounts/{id}/withdraw", (Guid id, [FromBody] WithdrawCommand command) =>
{

});

app.MapPost("/accounts/{id}/transfer", (Guid id, [FromBody] TransferCommand command) =>
{

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
