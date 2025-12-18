using Application.Features;
using Domain.Account;
using Mediator;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var accounts = new List<AccountEntity>
{
    AccountEntity.Open("Alice", 1000),
};

app.MapGet("/accounts", async (ISender sender) =>
{
    var result = await sender.Send(new GetAccountsQuery());
    return Results.Ok(result);
});

app.MapPost("/accounts", async ([FromBody] CreateAccountCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return Results.Created($"/accounts/{result.Id}", result);
});

app.MapGet("/accounts/{id}", async (Guid id, ISender sender) =>
{
    var result = await sender.Send(new GetAccountQuery(id));
    return result != null ? Results.Ok(result) : Results.NotFound();
});

app.MapPost("/accounts/{id}/deposit", async (Guid id, [FromBody] DepositCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return Results.Ok(result);
});

app.MapPost("/accounts/{id}/withdraw", async (Guid id, [FromBody] WithdrawCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return Results.Ok(result);
});

app.MapPost("/accounts/{id}/transfer", async (Guid id, [FromBody] TransferCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return Results.Ok(result);
});

app.MapDelete("/accounts/{id}", async (Guid id, ISender sender) =>
{
    await sender.Send(new DeleteAccountCommand(id));
    return Results.NoContent();
});

app.Run();
