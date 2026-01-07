using Application;
using Application.Features;
using Api.Requests;
using Mediator;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureApplicationServices();

var app = builder.Build();

app.MapGet("/accounts", async ([FromServices] ISender sender) =>
{
    var result = await sender.Send(new GetAccountsQuery());
    return Results.Ok(result);
});

app.MapPost("/accounts", async ([FromBody] CreateAccountCommand command, [FromServices] ISender sender) =>
{
    var result = await sender.Send(command);
    return Results.Created($"/accounts/{result.Id}", result);
});

app.MapGet("/accounts/{id}", async ([FromRoute] Guid id, [FromServices] ISender sender) =>
{
    var result = await sender.Send(new GetAccountQuery(id));
    return result != null ? Results.Ok(result) : Results.NotFound();
});

app.MapPost("/accounts/{id}/deposit", async ([FromRoute] Guid id, [FromBody] DepositRequest request, [FromServices] ISender sender) =>
{
    var result = await sender.Send(new DepositCommand(id, request.Amount));
    return Results.Ok(result);
});

app.MapPost("/accounts/{id}/withdraw", async ([FromRoute] Guid id, [FromBody] WithdrawRequest request, [FromServices] ISender sender) =>
{
    var result = await sender.Send(new WithdrawCommand(id, request.Amount));
    return Results.Ok(result);
});

app.MapPost("/accounts/{id}/transfer", async ([FromRoute] Guid id, [FromBody] TransferRequest request, [FromServices] ISender sender) =>
{
    var result = await sender.Send(new TransferCommand(id, request.ToAccountNumber, request.Amount));
    return Results.Ok(result);
});

app.MapDelete("/accounts/{id}", async ([FromRoute] Guid id, [FromServices] ISender sender) =>
{
    await sender.Send(new DeleteAccountCommand(id));
    return Results.NoContent();
});

app.Run();
